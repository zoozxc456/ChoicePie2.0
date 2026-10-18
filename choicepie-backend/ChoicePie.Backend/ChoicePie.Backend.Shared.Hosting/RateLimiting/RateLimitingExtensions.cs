using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ChoicePie.Backend.Shared.Hosting.RateLimiting;

public static class RateLimitingExtensions
{
    /// <summary>健康檢查路徑前綴，不受速率限制。</summary>
    private const string HealthPathPrefix = "/health";

    extension(IServiceCollection services)
    {
        public IServiceCollection AddChoicePieRateLimiting(IConfiguration configuration)
        {
            var settings = configuration.GetSection(RateLimitingSettings.SectionName).Get<RateLimitingSettings>()
                           ?? new RateLimitingSettings();

            services.AddRateLimiter(options =>
            {
                options.OnRejected = (context, cancellationToken) =>
                {
                    TimeSpan? retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var value)
                        ? value
                        : null;
                    return new ValueTask(
                        RateLimitRejectionWriter.WriteAsync(context.HttpContext, retryAfter, cancellationToken));
                };

                options.AddPolicy(RateLimitPolicyNames.Auth,
                    context => FixedWindow(context, RateLimitPolicyNames.Auth, RateLimitPartitionKeyResolver.ResolveIp(context), settings.Auth));
                options.AddPolicy(RateLimitPolicyNames.Email,
                    context => FixedWindow(context, RateLimitPolicyNames.Email, RateLimitPartitionKeyResolver.Resolve(context), settings.Email));
                options.AddPolicy(RateLimitPolicyNames.Ai,
                    context => FixedWindow(context, RateLimitPolicyNames.Ai, RateLimitPartitionKeyResolver.Resolve(context), settings.Ai));
                options.AddPolicy(RateLimitPolicyNames.Write,
                    context => FixedWindow(context, RateLimitPolicyNames.Write, RateLimitPartitionKeyResolver.Resolve(context), settings.Write));

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    context.Request.Path.StartsWithSegments(HealthPathPrefix)
                        ? RateLimitPartition.GetNoLimiter("health")
                        : FixedWindow(context, "global", RateLimitPartitionKeyResolver.Resolve(context), settings.Global));
            });

            return services;
        }
    }

    // partition key 帶上 policy 名稱，避免不同 policy 共用同一個計數器。
    // 計數存在 Redis，多台後端共用同一份限制（見 RedisFixedWindowRateLimiter）。
    private static RateLimitPartition<string> FixedWindow(HttpContext context, string policyName, string key, RateLimitRule rule)
    {
        var redis = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
        return RateLimitPartition.Get($"{policyName}:{key}",
            partitionKey => new RedisFixedWindowRateLimiter(partitionKey, rule, redis));
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ChoicePie.Backend.Shared.Hosting.HealthChecks;

public static class HealthCheckExtensions
{
    public const string LiveTag = "live";
    public const string ReadyTag = "ready";
    public const string LivePath = "/health/live";
    public const string ReadyPath = "/health/ready";

    extension(IServiceCollection services)
    {
        /// <summary>
        /// 註冊 live（僅代表 process 存活）與 Redis 的 ready 檢查；其他相依（如 PostgreSQL）
        /// 由呼叫端對回傳的 builder 以 <see cref="ReadyTag"/> 補上。
        /// </summary>
        public IHealthChecksBuilder AddChoicePieHealthChecks()
            => services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(), tags: [LiveTag])
                .AddCheck<DistributedCacheHealthCheck>("redis", tags: [ReadyTag]);
    }

    extension(IEndpointRouteBuilder endpoints)
    {
        /// <summary>匿名、位於 /api/v1 之外的健康檢查端點。</summary>
        public IEndpointRouteBuilder MapChoicePieHealthChecks()
        {
            endpoints.MapHealthChecks(LivePath, new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains(LiveTag),
                ResponseWriter = HealthCheckResponseWriter.WriteAsync
            }).AllowAnonymous();

            endpoints.MapHealthChecks(ReadyPath, new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains(ReadyTag),
                ResponseWriter = HealthCheckResponseWriter.WriteAsync
            }).AllowAnonymous();

            return endpoints;
        }
    }
}

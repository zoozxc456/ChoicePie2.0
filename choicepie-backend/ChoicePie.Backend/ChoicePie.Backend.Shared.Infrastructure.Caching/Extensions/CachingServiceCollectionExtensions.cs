using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Infrastructure.Caching.AppSettings;
using ChoicePie.Backend.Shared.Infrastructure.Caching.Services;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChoicePie.Backend.Shared.Infrastructure.Caching.Extensions;

public static class CachingServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddChoicePieCaching(IConfiguration configuration)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration[
                    $"{RedisConnectionSetting.SectionName}:{nameof(RedisConnectionSetting.ConnectionString)}"];
                options.InstanceName = "ChoicePie_";
            });

            services.AddHybridCache(options => options.DefaultEntryOptions = DefaultHybridCacheEntryOptions);
            services.AddScoped<ICachingService, HybridCachingService>();

            return services;
        }
    }

    private static HybridCacheEntryOptions DefaultHybridCacheEntryOptions => new()
    {
        Expiration = TimeSpan.FromMinutes(30),
        LocalCacheExpiration = TimeSpan.FromMinutes(3)
    };
}
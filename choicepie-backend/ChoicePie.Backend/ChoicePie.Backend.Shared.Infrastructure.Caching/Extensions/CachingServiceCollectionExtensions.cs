using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Infrastructure.Caching.AppSettings;
using ChoicePie.Backend.Shared.Infrastructure.Caching.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace ChoicePie.Backend.Shared.Infrastructure.Caching.Extensions;

public static class CachingServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        // 後端會跑多台：HybridCache 的實作換成 FusionCache，靠 Redis backplane 把 Remove/RemoveByTag
        // 同步到其他節點的 L1。程式碼仍只依賴 HybridCache / ICachingService。
        public IServiceCollection AddChoicePieCaching(IConfiguration configuration)
        {
            // 用 options callback 延後讀取連線字串，整合測試以 UseSetting 覆寫的值才會生效。
            string? RedisConnectionString() => configuration[
                $"{RedisConnectionSetting.SectionName}:{nameof(RedisConnectionSetting.ConnectionString)}"];

            // IDistributedCache 也給 DistributedCacheHealthCheck 使用。
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = RedisConnectionString();
                options.InstanceName = "ChoicePie_";
            });

            services.AddFusionCacheStackExchangeRedisBackplane(options =>
                options.Configuration = RedisConnectionString());

            services.AddFusionCache()
                .WithDefaultEntryOptions(DefaultEntryOptions)
                .WithSerializer(new FusionCacheSystemTextJsonSerializer())
                .WithRegisteredDistributedCache()
                .WithRegisteredBackplane()
                .AsHybridCache();

            services.AddScoped<ICachingService, HybridCachingService>();

            return services;
        }
    }

    // HybridCache adapter 只接受 HybridCacheEntryOptions（Expiration / LocalCacheExpiration），
    // fail-safe 與 timeout 只能在這裡設成全域預設。
    private static FusionCacheEntryOptions DefaultEntryOptions => new()
    {
        Duration = TimeSpan.FromMinutes(30),

        // 資料來源失敗或太慢時先回上一份值，避免整批請求一起失敗。
        IsFailSafeEnabled = true,
        FailSafeMaxDuration = TimeSpan.FromHours(2),
        FailSafeThrottleDuration = TimeSpan.FromSeconds(30),
        // 只有在手上有舊值可回時才會套用；沒有舊值就照常等 factory 完成。
        FactorySoftTimeout = TimeSpan.FromSeconds(1),

        // Redis 掛掉時不讓請求失敗，直接往下查資料來源。
        DistributedCacheSoftTimeout = TimeSpan.FromSeconds(1),
        DistributedCacheHardTimeout = TimeSpan.FromSeconds(2),
        AllowBackgroundDistributedCacheOperations = false,
        ReThrowDistributedCacheExceptions = false
    };
}

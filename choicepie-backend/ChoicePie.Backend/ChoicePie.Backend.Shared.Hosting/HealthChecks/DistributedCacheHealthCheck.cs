using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ChoicePie.Backend.Shared.Hosting.HealthChecks;

/// <summary>
/// 透過既有的 <see cref="IDistributedCache"/>（Redis）讀一個不存在的 key 來確認連線可用，
/// 不另外建立 Redis 連線，也沿用現有 RedisConnection 設定。
/// </summary>
public class DistributedCacheHealthCheck(IDistributedCache cache) : IHealthCheck
{
    private const string ProbeKey = "health-probe";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await cache.GetAsync(ProbeKey, cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis 無法連線", ex);
        }
    }
}

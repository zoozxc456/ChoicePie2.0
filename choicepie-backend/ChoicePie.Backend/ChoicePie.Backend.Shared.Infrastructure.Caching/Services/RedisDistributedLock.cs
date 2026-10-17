using System.Diagnostics;
using ChoicePie.Backend.Shared.Application.Interfaces;
using StackExchange.Redis;

namespace ChoicePie.Backend.Shared.Infrastructure.Caching.Services;

public sealed class RedisDistributedLockOptions
{
    /// <summary>持有者當機時，鎖最多卡住這麼久就自動過期。必須遠大於受保護區段的執行時間。</summary>
    public TimeSpan LeaseDuration { get; init; } = TimeSpan.FromSeconds(15);

    public TimeSpan AcquireTimeout { get; init; } = TimeSpan.FromSeconds(10);
}

/// <summary>
/// 單一 Redis 上的 SET NX PX 鎖。每個 lease 帶自己的 token，釋放時只刪掉仍屬於自己的鎖，
/// 避免 lease 過期後才釋放時，誤刪別的節點剛拿到的鎖。
/// </summary>
public sealed class RedisDistributedLock(IConnectionMultiplexer redis, RedisDistributedLockOptions options)
    : IDistributedLock
{
    private const string KeyPrefix = "ChoicePie:lock:";
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(10);

    private static readonly LuaScript ReleaseScript = LuaScript.Prepare("""
        if redis.call('get', @key) == @token then
            return redis.call('del', @key)
        end
        return 0
        """);

    public async Task<IAsyncDisposable> AcquireAsync(string resource, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        var key = new RedisKey(KeyPrefix + resource);
        var token = Guid.NewGuid().ToString("N");
        var stopwatch = Stopwatch.StartNew();

        while (!await db.StringSetAsync(key, token, options.LeaseDuration, When.NotExists))
        {
            if (stopwatch.Elapsed >= options.AcquireTimeout)
            {
                throw new TimeoutException($"等待分散式鎖 {resource} 超過 {options.AcquireTimeout.TotalSeconds} 秒。");
            }

            // 加一點隨機延遲，避免多個等待者每次都同時重試。
            await Task.Delay(RetryDelay + TimeSpan.FromMilliseconds(Random.Shared.Next(10)), ct);
        }

        return new Lease(db, key, token);
    }

    public async Task<IAsyncDisposable?> TryAcquireAsync(
        string resource, TimeSpan leaseDuration, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        var key = new RedisKey(KeyPrefix + resource);
        var token = Guid.NewGuid().ToString("N");

        return await db.StringSetAsync(key, token, leaseDuration, When.NotExists)
            ? new Lease(db, key, token)
            : null;
    }

    private sealed class Lease(IDatabase db, RedisKey key, string token) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            try
            {
                await db.ScriptEvaluateAsync(ReleaseScript, new { key, token });
            }
            catch (RedisException)
            {
                // 受保護的工作已經完成；釋放失敗只代表鎖要等 lease 過期，不該讓整個請求變成失敗。
            }
        }
    }
}

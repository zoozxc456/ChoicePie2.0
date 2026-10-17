using System.Diagnostics;
using System.Threading.RateLimiting;
using StackExchange.Redis;

namespace ChoicePie.Backend.Shared.Hosting.RateLimiting;

/// <summary>
/// 計數存在 Redis 的固定視窗限流器：多台後端共用同一份計數，限制不會隨台數放大。
/// INCR 與設定過期在同一個 Lua 腳本裡執行，同時湧入的請求也不會多放行。
/// Redis 連不上或太慢時退回這台自己的記憶體計數，寧可暫時放寬成「每台各自限制」，也不讓所有請求失敗。
/// </summary>
public sealed class RedisFixedWindowRateLimiter : RateLimiter
{
    private const string KeyPrefix = "ChoicePie:ratelimit:";

    // Redis 卡住時不能拖慢每一個請求，超過就改用本機計數。
    private static readonly TimeSpan RedisTimeout = TimeSpan.FromMilliseconds(500);

    private static readonly LuaScript IncrementScript = LuaScript.Prepare("""
        local count = redis.call('INCR', @key)
        local ttl = redis.call('PTTL', @key)
        if ttl < 0 then
            redis.call('PEXPIRE', @key, @windowMs)
            ttl = tonumber(@windowMs)
        end
        return { count, ttl }
        """);

    private readonly RedisKey _key;
    private readonly int _permitLimit;
    private readonly long _windowMs;
    private readonly IConnectionMultiplexer _redis;
    private readonly FixedWindowRateLimiter _fallback;
    private long _lastUsedTimestamp = Stopwatch.GetTimestamp();

    public RedisFixedWindowRateLimiter(string partitionKey, RateLimitRule rule, IConnectionMultiplexer redis)
    {
        _key = KeyPrefix + partitionKey;
        _permitLimit = rule.PermitLimit;
        _windowMs = (long)TimeSpan.FromSeconds(rule.WindowSeconds).TotalMilliseconds;
        _redis = redis;
        _fallback = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
        {
            PermitLimit = rule.PermitLimit,
            Window = TimeSpan.FromSeconds(rule.WindowSeconds),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    }

    // PartitionedRateLimiter 依這個值清掉閒置的分區（例如來過一次的 IP），不回傳的話分區會無限累積。
    public override TimeSpan? IdleDuration => Stopwatch.GetElapsedTime(Interlocked.Read(ref _lastUsedTimestamp));

    public override RateLimiterStatistics? GetStatistics() => null;

    // 同步路徑無法等 Redis：回傳未取得，middleware 會接著呼叫 AcquireAsync。
    protected override RateLimitLease AttemptAcquireCore(int permitCount) =>
        permitCount == 0 ? Lease.Acquired : Lease.NotAcquiredWithoutRetryAfter;

    protected override async ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken)
    {
        Interlocked.Exchange(ref _lastUsedTimestamp, Stopwatch.GetTimestamp());
        if (permitCount == 0)
        {
            return Lease.Acquired;
        }

        try
        {
            var result = (RedisResult[])(await _redis.GetDatabase()
                .ScriptEvaluateAsync(IncrementScript, new { key = _key, windowMs = _windowMs })
                .WaitAsync(RedisTimeout, cancellationToken))!;

            var count = (long)result[0];
            return count <= _permitLimit
                ? Lease.Acquired
                : Lease.Rejected(TimeSpan.FromMilliseconds((long)result[1]));
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException)
        {
            return await _fallback.AcquireAsync(permitCount, cancellationToken);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _fallback.Dispose();
        }

        base.Dispose(disposing);
    }

    private sealed class Lease(bool isAcquired, TimeSpan? retryAfter) : RateLimitLease
    {
        public static readonly Lease Acquired = new(true, null);
        public static readonly Lease NotAcquiredWithoutRetryAfter = new(false, null);

        public static Lease Rejected(TimeSpan retryAfter) => new(false, retryAfter);

        public override bool IsAcquired => isAcquired;

        public override IEnumerable<string> MetadataNames =>
            retryAfter is null ? [] : [MetadataName.RetryAfter.Name];

        public override bool TryGetMetadata(string metadataName, out object? metadata)
        {
            if (retryAfter is not null && metadataName == MetadataName.RetryAfter.Name)
            {
                metadata = retryAfter.Value;
                return true;
            }

            metadata = null;
            return false;
        }
    }
}

using ChoicePie.Backend.Shared.Infrastructure.Caching.Extensions;
using ChoicePie.Backend.Shared.Infrastructure.Caching.Services;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Redis;

namespace ChoicePie.Backend.Shared.Infrastructure.Caching.Tests.Services;

/// <summary>
/// 用正式的 AddChoicePieCaching 建兩個獨立的 ServiceProvider 模擬兩台後端，共用一個 Testcontainers Redis，
/// 驗證一台的 Remove/RemoveByTag 會經由 backplane 讓另一台的 L1 失效。
/// </summary>
[TestFixture]
public class CrossNodeInvalidationTests
{
    private static readonly TimeSpan PropagationTimeout = TimeSpan.FromSeconds(10);

    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    private ServiceProvider _nodeAProvider = null!;
    private ServiceProvider _nodeBProvider = null!;
    private HybridCachingService _nodeA = null!;
    private HybridCachingService _nodeB = null!;

    [OneTimeSetUp]
    public Task OneTimeSetUp() => _redis.StartAsync();

    [OneTimeTearDown]
    public async Task OneTimeTearDown() => await _redis.DisposeAsync();

    [SetUp]
    public async Task SetUp()
    {
        (_nodeAProvider, _nodeA) = CreateNode();
        (_nodeBProvider, _nodeB) = CreateNode();
        await WaitUntilBackplaneConnectedAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _nodeAProvider.DisposeAsync();
        await _nodeBProvider.DisposeAsync();
    }

    [Test]
    public async Task GetOrCreateAsync_GivenValueCachedByNodeA_WhenNodeBReadsSameKey_ThenServesFromRedisWithoutFactory()
    {
        var key = NewKey();
        await _nodeA.GetOrCreateAsync(key, _ => ValueTask.FromResult("from-a"));

        var nodeBFactoryCalls = 0;
        var result = await _nodeB.GetOrCreateAsync(key, _ =>
        {
            nodeBFactoryCalls++;
            return ValueTask.FromResult("from-b");
        });

        Assert.That(result, Is.EqualTo("from-a"));
        Assert.That(nodeBFactoryCalls, Is.Zero, "node B 應該從 Redis L2 讀到 node A 寫的值");
    }

    [Test]
    public async Task RemoveAsync_GivenKeyInBothNodesL1_WhenNodeARemoves_ThenNodeBRecomputes()
    {
        var key = NewKey();
        await WarmBothNodesAsync(key, "stale");

        await _nodeA.RemoveAsync(key);

        Assert.That(await NodeBEventuallyRecomputesAsync(key), Is.True,
            "node B 的 L1 應該在 backplane 通知後失效");
    }

    [Test]
    public async Task RemoveByTagAsync_GivenTaggedKeysInBothNodesL1_WhenNodeARemovesTag_ThenNodeBRecomputesAll()
    {
        var tag = $"tag-{Guid.NewGuid():N}";
        var firstKey = NewKey();
        var secondKey = NewKey();
        await WarmBothNodesAsync(firstKey, "stale", tag);
        await WarmBothNodesAsync(secondKey, "stale", tag);

        await _nodeA.RemoveByTagAsync(tag);

        Assert.That(await NodeBEventuallyRecomputesAsync(firstKey, tag), Is.True);
        Assert.That(await NodeBEventuallyRecomputesAsync(secondKey, tag), Is.True);
    }

    [Test]
    public async Task RemoveByTagAsync_GivenEntryWithDifferentTagInNodeBL1_WhenNodeARemovesTag_ThenNodeBKeepsIt()
    {
        var untouchedKey = NewKey();
        await WarmBothNodesAsync(untouchedKey, "kept", "other-tag");

        await _nodeA.RemoveByTagAsync($"tag-{Guid.NewGuid():N}");
        await Task.Delay(TimeSpan.FromMilliseconds(500));

        var result = await _nodeB.GetOrCreateAsync(untouchedKey, _ => ValueTask.FromResult("fresh"), tags: ["other-tag"]);
        Assert.That(result, Is.EqualTo("kept"));
    }

    private (ServiceProvider Provider, HybridCachingService Cache) CreateNode()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RedisConnection:ConnectionString"] = _redis.GetConnectionString()
            })
            .Build();

        var services = new ServiceCollection();
        services.AddChoicePieCaching(configuration);
        var provider = services.BuildServiceProvider();
        return (provider, new HybridCachingService(provider.GetRequiredService<HybridCache>()));
    }

    // backplane 的 Redis 訂閱是非同步建立的，訂閱好之前送出的訊息會遺失。
    // 每次用新的 probe key 重試，直到 node B 確實收到 node A 的 Remove 為止。
    private async Task WaitUntilBackplaneConnectedAsync()
    {
        var connected = await EventuallyAsync(async () =>
        {
            var probe = NewKey();
            await WarmBothNodesAsync(probe, "stale");
            await _nodeA.RemoveAsync(probe);
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            return await _nodeB.GetOrCreateAsync(probe, _ => ValueTask.FromResult("fresh")) == "fresh";
        });

        if (!connected) Assert.Fail("backplane 在時限內沒有連上");
    }

    // 兩台都透過 GetOrCreate 把值放進自己的 L1。
    private async Task WarmBothNodesAsync(string key, string value, string? tag = null)
    {
        string[]? tags = tag is null ? null : [tag];
        await _nodeA.GetOrCreateAsync(key, _ => ValueTask.FromResult(value), tags: tags);
        await _nodeB.GetOrCreateAsync(key, _ => ValueTask.FromResult(value), tags: tags);
    }

    // L1 還沒失效就會回舊值且不跑 factory；失效後才會跑 factory 拿到 "fresh"。
    private Task<bool> NodeBEventuallyRecomputesAsync(string key, string? tag = null)
    {
        string[]? tags = tag is null ? null : [tag];
        return EventuallyAsync(async () =>
            await _nodeB.GetOrCreateAsync(key, _ => ValueTask.FromResult("fresh"), tags: tags) == "fresh");
    }

    private static async Task<bool> EventuallyAsync(Func<Task<bool>> condition)
    {
        var deadline = DateTime.UtcNow + PropagationTimeout;
        while (DateTime.UtcNow < deadline)
        {
            if (await condition()) return true;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }

        return false;
    }

    private static string NewKey() => $"cross-node:{Guid.NewGuid():N}";
}

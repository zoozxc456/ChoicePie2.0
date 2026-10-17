using ChoicePie.Backend.Shared.Application.Abstractions.Caching;
using ChoicePie.Backend.Shared.Infrastructure.Caching.Services;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;

namespace ChoicePie.Backend.Shared.Infrastructure.Caching.Tests.Services;

[TestFixture]
public class HybridCachingServiceTests
{
    private ServiceProvider _serviceProvider = null!;
    private HybridCachingService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        // 正式環境的 HybridCache 是 FusionCache 的 adapter；這裡不接 Redis，只用 L1 記憶體，
        // 不需要真的起一個 Redis 就能測 HybridCachingService 在 adapter 底下的行為。
        var services = new ServiceCollection();
        services.AddFusionCache().AsHybridCache();
        _serviceProvider = services.BuildServiceProvider();
        _sut = new HybridCachingService(_serviceProvider.GetRequiredService<HybridCache>());
    }

    [TearDown]
    public void TearDown() => _serviceProvider.Dispose();

    [Test]
    public async Task GetOrCreateAsync_GivenNeverSetKey_WhenCalled_ThenInvokesFactoryAndCachesResult()
    {
        var factoryCallCount = 0;

        var first = await _sut.GetOrCreateAsync("computed-key", _ =>
        {
            factoryCallCount++;
            return ValueTask.FromResult("computed value");
        });
        var second = await _sut.GetOrCreateAsync("computed-key", _ =>
        {
            factoryCallCount++;
            return ValueTask.FromResult("computed value");
        });

        Assert.That(first, Is.EqualTo("computed value"));
        Assert.That(second, Is.EqualTo("computed value"));
        Assert.That(factoryCallCount, Is.EqualTo(1), "第二次應該直接命中快取，不會再呼叫 factory");
    }

    [Test]
    public async Task RemoveAsync_GivenCachedKey_WhenCalled_ThenNextGetOrCreateRecomputes()
    {
        await _sut.GetOrCreateAsync("removable-key", _ => ValueTask.FromResult("stale"));

        await _sut.RemoveAsync("removable-key");
        var result = await _sut.GetOrCreateAsync("removable-key", _ => ValueTask.FromResult("fresh"));

        Assert.That(result, Is.EqualTo("fresh"));
    }

    [Test]
    public async Task RemoveByTagAsync_GivenEntriesSharingATag_WhenCalled_ThenRecomputesAllOfThem()
    {
        await _sut.GetOrCreateAsync("tagged-key-1", _ => ValueTask.FromResult("stale"), tags: ["shared-tag"]);
        await _sut.GetOrCreateAsync("tagged-key-2", _ => ValueTask.FromResult("stale"), tags: ["shared-tag"]);

        await _sut.RemoveByTagAsync("shared-tag");

        Assert.That(await _sut.GetOrCreateAsync("tagged-key-1", _ => ValueTask.FromResult("fresh"), tags: ["shared-tag"]),
            Is.EqualTo("fresh"));
        Assert.That(await _sut.GetOrCreateAsync("tagged-key-2", _ => ValueTask.FromResult("fresh"), tags: ["shared-tag"]),
            Is.EqualTo("fresh"));
    }

    [Test]
    public async Task RemoveByTagAsync_GivenEntryWithOtherTag_WhenCalled_ThenKeepsIt()
    {
        await _sut.GetOrCreateAsync("other-key", _ => ValueTask.FromResult("kept"), tags: ["other-tag"]);

        await _sut.RemoveByTagAsync("shared-tag");

        Assert.That(await _sut.GetOrCreateAsync("other-key", _ => ValueTask.FromResult("fresh"), tags: ["other-tag"]),
            Is.EqualTo("kept"));
    }

    [Test]
    public async Task GetOrCreateAsync_GivenCustomExpirationOptions_WhenCalledTwice_ThenSecondCallHitsCache()
    {
        var options = new ChoicePieCacheOptions
        {
            Expiration = TimeSpan.FromMinutes(10),
            LocalCacheExpiration = TimeSpan.FromMinutes(1)
        };

        await _sut.GetOrCreateAsync("custom-options-key", _ => ValueTask.FromResult("value"), options);
        var result = await _sut.GetOrCreateAsync("custom-options-key", _ => ValueTask.FromResult("other"), options);

        Assert.That(result, Is.EqualTo("value"));
    }
}

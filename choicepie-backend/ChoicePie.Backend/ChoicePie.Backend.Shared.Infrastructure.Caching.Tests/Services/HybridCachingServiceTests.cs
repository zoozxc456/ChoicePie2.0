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
    public async Task SetAsync_ThenGetAsync_GivenSameKey_WhenCalled_ThenReturnsStoredValue()
    {
        await _sut.SetAsync("key-1", "hello world");

        var result = await _sut.GetAsync<string>("key-1");

        Assert.That(result, Is.EqualTo("hello world"));
    }

    [Test]
    public async Task GetAsync_GivenNeverSetKey_WhenCalled_ThenReturnsDefaultWithoutWritingToCache()
    {
        var result = await _sut.GetAsync<string>("never-set-key");

        Assert.That(result, Is.Null);
    }

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
    public async Task RemoveAsync_GivenPreviouslySetKey_WhenCalled_ThenSubsequentGetReturnsDefault()
    {
        await _sut.SetAsync("removable-key", "value");

        await _sut.RemoveAsync("removable-key");
        var result = await _sut.GetAsync<string>("removable-key");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task RemoveByTagAsync_GivenEntriesSharingATag_WhenCalled_ThenRemovesAllOfThem()
    {
        await _sut.SetAsync("tagged-key-1", "a", tags: ["shared-tag"]);
        await _sut.SetAsync("tagged-key-2", "b", tags: ["shared-tag"]);

        await _sut.RemoveByTagAsync("shared-tag");

        Assert.That(await _sut.GetAsync<string>("tagged-key-1"), Is.Null);
        Assert.That(await _sut.GetAsync<string>("tagged-key-2"), Is.Null);
    }

    [Test]
    public async Task SetAsync_GivenNullChoicePieCacheOptions_WhenCalled_ThenStillCachesUsingHybridCacheDefaults()
    {
        await _sut.SetAsync("no-options-key", "value", options: null);

        var result = await _sut.GetAsync<string>("no-options-key");

        Assert.That(result, Is.EqualTo("value"));
    }

    [Test]
    public async Task SetAsync_GivenCustomExpirationOptions_WhenCalled_ThenValueIsRetrievableBeforeExpiry()
    {
        var options = new ChoicePieCacheOptions
        {
            Expiration = TimeSpan.FromMinutes(10),
            LocalCacheExpiration = TimeSpan.FromMinutes(1)
        };

        await _sut.SetAsync("custom-options-key", "value", options);

        Assert.That(await _sut.GetAsync<string>("custom-options-key"), Is.EqualTo("value"));
    }
}

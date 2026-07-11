using ChoicePie.Backend.Shared.Application.Abstractions.Caching;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using Microsoft.Extensions.Caching.Hybrid;

namespace ChoicePie.Backend.Shared.Infrastructure.Caching.Services;

public class HybridCachingService(HybridCache cache) : ICachingService, IScopedDependency
{
    public ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        ChoicePieCacheOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken ct = default)
    {
        var entryOptions = MapOptions(options);
        return cache.GetOrCreateAsync(key, factory, entryOptions, tags, ct);
    }

    public async ValueTask<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var factoryRan = false;
        var result = await cache.GetOrCreateAsync<T?>(
            key,
            ct2 =>
            {
                factoryRan = true;
                return ValueTask.FromResult<T?>(default);
            },
            new HybridCacheEntryOptions
            {
                Flags = HybridCacheEntryFlags.DisableLocalCacheWrite |
                        HybridCacheEntryFlags.DisableDistributedCacheWrite
            },
            cancellationToken: ct
        );
        return factoryRan ? default : result;
    }

    public ValueTask SetAsync<T>(
        string key,
        T value,
        ChoicePieCacheOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken ct = default)
    {
        var entryOptions = MapOptions(options);
        return cache.SetAsync(key, value, entryOptions, tags, ct);
    }

    public ValueTask RemoveAsync(string key, CancellationToken ct = default)
    {
        return cache.RemoveAsync(key, ct);
    }

    public ValueTask RemoveByTagAsync(string tag, CancellationToken ct = default)
    {
        return cache.RemoveByTagAsync(tag, ct);
    }

    private static HybridCacheEntryOptions? MapOptions(ChoicePieCacheOptions? auraOptions)
    {
        if (auraOptions == null) return null;

        return new HybridCacheEntryOptions
        {
            Expiration = auraOptions.Expiration,
            LocalCacheExpiration = auraOptions.LocalCacheExpiration
        };
    }
}
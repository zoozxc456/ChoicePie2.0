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
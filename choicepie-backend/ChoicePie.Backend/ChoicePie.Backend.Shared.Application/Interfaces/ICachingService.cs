using ChoicePie.Backend.Shared.Application.Abstractions.Caching;

namespace ChoicePie.Backend.Shared.Application.Interfaces;

public interface ICachingService
{
    ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        ChoicePieCacheOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken ct = default);

    ValueTask<T?> GetAsync<T>(string key, CancellationToken ct = default);

    ValueTask SetAsync<T>(
        string key,
        T value,
        ChoicePieCacheOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken ct = default);

    ValueTask RemoveAsync(string key, CancellationToken ct = default);

    ValueTask RemoveByTagAsync(string tag, CancellationToken ct = default);
}
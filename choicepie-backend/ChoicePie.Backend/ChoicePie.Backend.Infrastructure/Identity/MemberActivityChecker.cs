using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Shared.Application.Abstractions.Caching;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;

namespace ChoicePie.Backend.Infrastructure.Identity;

/// <summary>
/// 以輕量投影判斷會員是否仍可使用（存在、未軟刪除、目前未停權），結果以會員為單位快取（只存 bool）。
/// 快取 TTL 很短作為保底；會員被刪除／停權／解除停權時由領域事件處理器主動 <see cref="InvalidateAsync"/>。
/// </summary>
public sealed class MemberActivityChecker(
    IReadRepository readRepository,
    ICachingService cachingService,
    TimeProvider timeProvider)
    : IMemberActivityChecker, IMemberActivityCache, IScopedDependency
{
    private static readonly ChoicePieCacheOptions CacheOptions = new()
    {
        Expiration = TimeSpan.FromSeconds(60),
        // L1 要比總 TTL 短：多節點部署時，其他節點的本地快取最多晚這麼久才會看到失效。
        LocalCacheExpiration = TimeSpan.FromSeconds(10)
    };

    public async Task<bool> IsActiveAsync(Guid memberId, CancellationToken cancellationToken) =>
        await cachingService.GetOrCreateAsync(
            BuildKey(memberId),
            _ => ValueTask.FromResult(QueryIsActive(memberId)),
            CacheOptions,
            ct: cancellationToken);

    public async Task InvalidateAsync(Guid memberId, CancellationToken cancellationToken) =>
        await cachingService.RemoveAsync(BuildKey(memberId), cancellationToken);

    private bool QueryIsActive(Guid memberId)
    {
        var member = readRepository.Query<Member>()
            .Where(m => m.Id == memberId)
            .Select(m => new { m.DeletedAt, m.IsSuspended, m.SuspendedUntil })
            .FirstOrDefault();

        if (member is null || member.DeletedAt.HasValue)
        {
            return false;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var isSuspended = member.IsSuspended && (member.SuspendedUntil is null || member.SuspendedUntil.Value > now);
        return !isSuspended;
    }

    internal static string BuildKey(Guid memberId) => $"MemberActivity:{memberId}";
}

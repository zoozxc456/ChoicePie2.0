using ChoicePie.Backend.Application.AdminQuizzes.Dtos;
using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Shared.Application.Abstractions.Caching;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Application.Interfaces;

namespace ChoicePie.Backend.Infrastructure.Caching.Quizzes;

/// <summary>
/// 在 <see cref="IQuizQueryService"/> 外面加一層 cache-aside。只快取與呼叫者無關的資料；
/// 誰能看到題目內容等授權判斷仍在 Query handler 裡做，所以快取裡永遠是完整資料。
/// 失效由 <see cref="QuizCacheInvalidator"/> 透過 tag 處理；作答與分享次數則等 TTL 自然更新。
/// </summary>
/// <remarks>
/// 刻意不實作 DI marker interface：Scrutor 掃描會把它也註冊成 IQuizQueryService，
/// 改由 Program.cs 以 Decorate 包住 QuizQueryService。
/// </remarks>
public sealed class CachedQuizQueryService(IQuizQueryService inner, ICachingService cache) : IQuizQueryService
{
    // 有 backplane 同步各節點的 L1，所以 L1 與 L2 用同一個 TTL。
    private static readonly ChoicePieCacheOptions QuizOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromMinutes(5)
    };

    private static readonly ChoicePieCacheOptions TagsOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(10)
    };

    public async Task<QuizDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await cache.GetOrCreateAsync(
            QuizCacheKeys.Detail(id),
            async ct => await inner.GetByIdAsync(id, ct),
            QuizOptions,
            [QuizCacheKeys.QuizTag(id)],
            cancellationToken);

    public async Task<QuizForAttemptDto?> GetForAttemptAsync(Guid id, CancellationToken cancellationToken) =>
        await cache.GetOrCreateAsync(
            QuizCacheKeys.ForAttempt(id),
            async ct => await inner.GetForAttemptAsync(id, ct),
            QuizOptions,
            [QuizCacheKeys.QuizTag(id)],
            cancellationToken);

    public async Task<IReadOnlyList<string>> GetTagsAsync(CancellationToken cancellationToken) =>
        await cache.GetOrCreateAsync(
            QuizCacheKeys.Tags,
            async ct => await inner.GetTagsAsync(ct),
            TagsOptions,
            [QuizCacheKeys.CatalogTag],
            cancellationToken);

    // 清單裡其他題庫的標題或創作者改名不會讓這筆失效，最多舊 5 分鐘。
    public async Task<IReadOnlyList<QuizSummaryDto>> GetRelatedAsync(Guid quizId, int limit,
        CancellationToken cancellationToken) =>
        await cache.GetOrCreateAsync(
            QuizCacheKeys.Related(quizId, limit),
            async ct => await inner.GetRelatedAsync(quizId, limit, ct),
            QuizOptions,
            [QuizCacheKeys.QuizTag(quizId), QuizCacheKeys.CatalogTag],
            cancellationToken);

    public Task<AdminQuizDetailDto?> AdminGetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        inner.AdminGetByIdAsync(id, cancellationToken);

    public Task<PagedResult<QuizSummaryDto>> ListAsync(string? tag, string? search, Guid? ownerId, int pageNumber,
        int pageSize, CancellationToken cancellationToken) =>
        inner.ListAsync(tag, search, ownerId, pageNumber, pageSize, cancellationToken);

    public Task<PagedResult<QuizSummaryDto>> AdminListAsync(string? search, int pageNumber, int pageSize,
        CancellationToken cancellationToken) =>
        inner.AdminListAsync(search, pageNumber, pageSize, cancellationToken);

    public Task<PagedResult<QuizSummaryDto>> ListFavoritesAsync(Guid userId, int pageNumber, int pageSize,
        CancellationToken cancellationToken) =>
        inner.ListFavoritesAsync(userId, pageNumber, pageSize, cancellationToken);

    public Task<PagedResult<QuizSummaryDto>> ListFollowingFeedAsync(Guid userId, int pageNumber, int pageSize,
        CancellationToken cancellationToken) =>
        inner.ListFollowingFeedAsync(userId, pageNumber, pageSize, cancellationToken);

    public Task<PagedResult<QuizSummaryDto>> ListByCreatorAsync(Guid creatorId, int pageNumber, int pageSize,
        CancellationToken cancellationToken) =>
        inner.ListByCreatorAsync(creatorId, pageNumber, pageSize, cancellationToken);

    public Task<AdminQuizDashboardStatsDto> AdminGetDashboardStatsAsync(CancellationToken cancellationToken) =>
        inner.AdminGetDashboardStatsAsync(cancellationToken);

    public Task<IReadOnlyList<QuizSummaryDto>> AdminGetTopQuizzesAsync(int limit, CancellationToken cancellationToken) =>
        inner.AdminGetTopQuizzesAsync(limit, cancellationToken);
}

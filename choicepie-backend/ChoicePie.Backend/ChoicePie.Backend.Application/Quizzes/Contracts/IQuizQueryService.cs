using ChoicePie.Backend.Application.AdminQuizzes.Dtos;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;

namespace ChoicePie.Backend.Application.Quizzes.Contracts;

public interface IQuizQueryService
{
    Task<QuizDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    // Admin-facing detail: includes takedown metadata and is not restricted by ownership,
    // unlike GetByIdAsync/GetQuizByIdQueryHandler which strips Questions for non-owners.
    Task<AdminQuizDetailDto?> AdminGetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<QuizForAttemptDto?> GetForAttemptAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<QuizSummaryDto>> ListAsync(
        string? tag, string? search, Guid? ownerId, int pageNumber, int pageSize,
        CancellationToken cancellationToken);

    // Admin-facing listing: not restricted to Published/own-Draft like ListAsync - returns
    // quizzes across every status (including TakenDown) so moderators can find anything.
    Task<PagedResult<QuizSummaryDto>> AdminListAsync(
        string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);

    // 會員收藏中仍為 Published 的測驗，依收藏時間新到舊。
    Task<PagedResult<QuizSummaryDto>> ListFavoritesAsync(
        Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    // 追蹤中創作者的 Published 測驗動態，新到舊。
    Task<PagedResult<QuizSummaryDto>> ListFollowingFeedAsync(
        Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    // 指定創作者的 Published 測驗，新到舊（公開）。
    Task<PagedResult<QuizSummaryDto>> ListByCreatorAsync(
        Guid creatorId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetTagsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<QuizSummaryDto>> GetRelatedAsync(Guid quizId, int limit, CancellationToken cancellationToken);

    Task<AdminQuizDashboardStatsDto> AdminGetDashboardStatsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<QuizSummaryDto>> AdminGetTopQuizzesAsync(int limit, CancellationToken cancellationToken);
}

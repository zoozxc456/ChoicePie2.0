using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;

namespace ChoicePie.Backend.Application.Quizzes.Contracts;

public interface IQuizQueryService
{
    Task<QuizDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<QuizForAttemptDto?> GetForAttemptAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<QuizSummaryDto>> ListAsync(
        string? tag, string? search, Guid? ownerId, int pageNumber, int pageSize,
        CancellationToken cancellationToken);

    // Admin-facing listing: not restricted to Published/own-Draft like ListAsync - returns
    // quizzes across every status (including TakenDown) so moderators can find anything.
    Task<PagedResult<QuizSummaryDto>> AdminListAsync(
        string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetTagsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<QuizSummaryDto>> GetRelatedAsync(Guid quizId, int limit, CancellationToken cancellationToken);
}

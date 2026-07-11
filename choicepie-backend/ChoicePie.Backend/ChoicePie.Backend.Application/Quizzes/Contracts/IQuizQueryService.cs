using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;

namespace ChoicePie.Backend.Application.Quizzes.Contracts;

public interface IQuizQueryService
{
    Task<QuizDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<QuizSummaryDto>> ListAsync(string? tag, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> GetTagsAsync(CancellationToken cancellationToken);
}

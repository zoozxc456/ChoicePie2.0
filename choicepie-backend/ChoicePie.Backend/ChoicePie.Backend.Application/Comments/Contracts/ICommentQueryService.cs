using ChoicePie.Backend.Application.Comments.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;

namespace ChoicePie.Backend.Application.Comments.Contracts;

public interface ICommentQueryService
{
    Task<PagedResult<CommentDto>> ListByQuizIdAsync(
        Guid quizId, int pageNumber, int pageSize, CancellationToken cancellationToken);
}

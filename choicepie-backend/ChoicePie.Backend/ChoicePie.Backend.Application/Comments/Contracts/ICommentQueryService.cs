using ChoicePie.Backend.Application.Comments.Dtos;

namespace ChoicePie.Backend.Application.Comments.Contracts;

public interface ICommentQueryService
{
    Task<IReadOnlyList<CommentDto>> ListByQuizIdAsync(Guid quizId, CancellationToken cancellationToken);
}

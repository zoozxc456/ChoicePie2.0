using ChoicePie.Backend.Application.AdminMembers.Dtos;
using ChoicePie.Backend.Application.Comments.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;

namespace ChoicePie.Backend.Application.Comments.Contracts;

public interface ICommentQueryService
{
    Task<PagedResult<CommentDto>> ListByQuizIdAsync(
        Guid quizId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    // Admin-facing: comments authored by a given member, with the quiz title joined in so
    // moderators can see which quiz each comment belongs to.
    Task<PagedResult<AdminMemberCommentDto>> ListByUserIdAsync(
        Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken);
}

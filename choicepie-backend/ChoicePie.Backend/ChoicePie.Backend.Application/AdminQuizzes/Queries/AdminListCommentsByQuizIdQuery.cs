using ChoicePie.Backend.Application.Comments.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.AdminQuizzes.Queries;

public sealed class AdminListCommentsByQuizIdQuery : PaginationParameters, IRequest<PagedResult<CommentDto>>
{
    public Guid QuizId { get; set; }
}

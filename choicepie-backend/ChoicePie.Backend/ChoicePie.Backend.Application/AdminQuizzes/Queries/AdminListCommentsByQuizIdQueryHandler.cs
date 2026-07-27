using ChoicePie.Backend.Application.Comments.Contracts;
using ChoicePie.Backend.Application.Comments.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.AdminQuizzes.Queries;

public sealed class AdminListCommentsByQuizIdQueryHandler(ICommentQueryService commentQueryService)
    : IRequestHandler<AdminListCommentsByQuizIdQuery, PagedResult<CommentDto>>
{
    public Task<PagedResult<CommentDto>> Handle(AdminListCommentsByQuizIdQuery request, CancellationToken cancellationToken) =>
        commentQueryService.ListByQuizIdAsync(request.QuizId, request.PageNumber, request.PageSize, cancellationToken);
}

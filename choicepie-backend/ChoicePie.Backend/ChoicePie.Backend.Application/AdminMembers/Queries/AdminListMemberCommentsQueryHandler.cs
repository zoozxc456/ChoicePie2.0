using ChoicePie.Backend.Application.AdminMembers.Dtos;
using ChoicePie.Backend.Application.Comments.Contracts;
using ChoicePie.Backend.Shared.Application.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.AdminMembers.Queries;

public sealed class AdminListMemberCommentsQueryHandler(ICommentQueryService commentQueryService)
    : IRequestHandler<AdminListMemberCommentsQuery, PagedResult<AdminMemberCommentDto>>
{
    public Task<PagedResult<AdminMemberCommentDto>> Handle(AdminListMemberCommentsQuery request, CancellationToken cancellationToken) =>
        commentQueryService.ListByUserIdAsync(request.MemberId, request.PageNumber, request.PageSize, cancellationToken);
}

using ChoicePie.Backend.Application.AdminMembers.Dtos;
using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Shared.Application.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.AdminMembers.Queries;

public sealed class AdminListMembersQueryHandler(IMemberQueryService memberQueryService)
    : IRequestHandler<AdminListMembersQuery, PagedResult<AdminMemberSummaryDto>>
{
    public Task<PagedResult<AdminMemberSummaryDto>> Handle(AdminListMembersQuery request, CancellationToken cancellationToken) =>
        memberQueryService.AdminListAsync(request.Search, request.PageNumber, request.PageSize, cancellationToken);
}

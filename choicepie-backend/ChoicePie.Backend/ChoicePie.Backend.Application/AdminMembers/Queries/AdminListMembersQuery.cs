using ChoicePie.Backend.Application.AdminMembers.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.AdminMembers.Queries;

public sealed class AdminListMembersQuery : PaginationParameters, IRequest<PagedResult<AdminMemberSummaryDto>>
{
    public string? Search { get; set; }
}

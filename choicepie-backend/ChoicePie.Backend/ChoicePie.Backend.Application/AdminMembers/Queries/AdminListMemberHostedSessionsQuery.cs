using ChoicePie.Backend.Application.GameSessions.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.AdminMembers.Queries;

public sealed class AdminListMemberHostedSessionsQuery : PaginationParameters, IRequest<PagedResult<GameSessionSummaryDto>>
{
    public Guid MemberId { get; set; }
}

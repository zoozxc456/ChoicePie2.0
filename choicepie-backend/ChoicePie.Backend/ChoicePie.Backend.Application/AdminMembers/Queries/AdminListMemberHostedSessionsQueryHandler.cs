using ChoicePie.Backend.Application.GameSessions.Contracts;
using ChoicePie.Backend.Application.GameSessions.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.AdminMembers.Queries;

public sealed class AdminListMemberHostedSessionsQueryHandler(IGameSessionQueryService gameSessionQueryService)
    : IRequestHandler<AdminListMemberHostedSessionsQuery, PagedResult<GameSessionSummaryDto>>
{
    public Task<PagedResult<GameSessionSummaryDto>> Handle(
        AdminListMemberHostedSessionsQuery request, CancellationToken cancellationToken) =>
        gameSessionQueryService.GetHostedByUserIdAsync(request.MemberId, request.PageNumber, request.PageSize, cancellationToken);
}

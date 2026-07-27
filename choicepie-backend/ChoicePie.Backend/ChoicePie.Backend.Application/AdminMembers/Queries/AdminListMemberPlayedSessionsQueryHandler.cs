using ChoicePie.Backend.Application.GameSessions.Contracts;
using ChoicePie.Backend.Application.GameSessions.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.AdminMembers.Queries;

public sealed class AdminListMemberPlayedSessionsQueryHandler(IGameSessionQueryService gameSessionQueryService)
    : IRequestHandler<AdminListMemberPlayedSessionsQuery, PagedResult<GameSessionSummaryDto>>
{
    public Task<PagedResult<GameSessionSummaryDto>> Handle(
        AdminListMemberPlayedSessionsQuery request, CancellationToken cancellationToken) =>
        gameSessionQueryService.GetPlayedByMemberIdAsync(request.MemberId, request.PageNumber, request.PageSize, cancellationToken);
}

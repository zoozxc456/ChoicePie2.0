using ChoicePie.Backend.Application.GameSessions.Contracts;
using ChoicePie.Backend.Application.GameSessions.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.GameSessions.Queries;

public sealed class GetPlayedGameSessionsQueryHandler(
    IGameSessionQueryService gameSessionQueryService,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetPlayedGameSessionsQuery, PagedResult<GameSessionSummaryDto>>
{
    public Task<PagedResult<GameSessionSummaryDto>> Handle(GetPlayedGameSessionsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthenticatedException();
        return gameSessionQueryService.GetPlayedByMemberIdAsync(userId, request.PageNumber, request.PageSize, cancellationToken);
    }
}

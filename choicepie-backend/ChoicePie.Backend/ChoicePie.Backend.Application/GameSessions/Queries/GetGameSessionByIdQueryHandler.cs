using ChoicePie.Backend.Application.GameSessions.Contracts;
using ChoicePie.Backend.Application.GameSessions.Dtos;
using ChoicePie.Backend.Domain.Aggregates.GameSession.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.GameSessions.Queries;

public sealed class GetGameSessionByIdQueryHandler(
    IGameSessionQueryService gameSessionQueryService,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetGameSessionByIdQuery, GameSessionDetailDto>
{
    public async Task<GameSessionDetailDto> Handle(GetGameSessionByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthenticatedException();

        var result = await gameSessionQueryService.GetByIdAsync(request.Id, userId, cancellationToken)
                     ?? throw new GameSessionNotFoundException(request.Id);

        if (!result.IsHost && result.MyRank is null)
        {
            throw new GameSessionForbiddenException(result.Id, userId);
        }

        return result;
    }
}

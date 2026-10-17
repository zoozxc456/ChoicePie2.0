using ChoicePie.Backend.Application.GameRooms.Dtos;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public sealed class RejoinAsPlayerCommandHandler(
    IGameRoomRepository gameRoomRepository, TimeProvider timeProvider)
    : IRequestHandler<RejoinAsPlayerCommand, RejoinAsPlayerResult>
{
    public async Task<RejoinAsPlayerResult> Handle(RejoinAsPlayerCommand request, CancellationToken cancellationToken)
    {
        var room = await gameRoomRepository.GetByRoomCodeAsync(request.RoomCode, cancellationToken)
                   ?? throw new RoomNotFoundException(request.RoomCode);

        var player = room.ReconnectPlayer(request.ReconnectToken, request.ConnectionId);

        await gameRoomRepository.SaveAsync(room, cancellationToken);

        var rank = room.GetRankings().First(r => r.PlayerId == player.Id).Rank;
        var playerDto = PlayerDto.ForPublic(player, rank, room.CurrentQuestionIndex);
        var roomState = GameRoomStateFactory.Build(room, timeProvider.GetUtcNow().UtcDateTime, forHost: false, player);

        return new RejoinAsPlayerResult(playerDto, roomState);
    }
}

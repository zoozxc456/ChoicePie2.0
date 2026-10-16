using ChoicePie.Backend.Application.GameRooms.Dtos;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public sealed class JoinRoomCommandHandler(IGameRoomRepository gameRoomRepository, TimeProvider timeProvider)
    : IRequestHandler<JoinRoomCommand, JoinRoomResult>
{
    public async Task<JoinRoomResult> Handle(JoinRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await gameRoomRepository.GetByRoomCodeAsync(request.RoomCode, cancellationToken)
                   ?? throw new RoomNotFoundException(request.RoomCode);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var player = room.Join(request.Nickname, request.ConnectionId, now, request.MemberId);

        await gameRoomRepository.SaveAsync(room, cancellationToken);

        var playerDto = PlayerDto.ForPublic(player, rank: room.Players.Count, room.CurrentQuestionIndex);
        var roomState = GameRoomStateFactory.Build(room, now, forHost: false);

        return new JoinRoomResult(playerDto, roomState, player.ReconnectToken);
    }
}

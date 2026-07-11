using ChoicePie.Backend.Application.GameRooms.Dtos;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public sealed class JoinRoomCommandHandler(IGameRoomRepository gameRoomRepository)
    : IRequestHandler<JoinRoomCommand, PlayerDto>
{
    public async Task<PlayerDto> Handle(JoinRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await gameRoomRepository.GetByRoomCodeAsync(request.RoomCode, cancellationToken)
                   ?? throw new RoomNotFoundException(request.RoomCode);

        var player = room.Join(request.Nickname, request.ConnectionId, DateTime.UtcNow);

        await gameRoomRepository.SaveAsync(room, cancellationToken);

        return PlayerDto.ForPublic(player, rank: room.Players.Count, room.CurrentQuestionIndex);
    }
}

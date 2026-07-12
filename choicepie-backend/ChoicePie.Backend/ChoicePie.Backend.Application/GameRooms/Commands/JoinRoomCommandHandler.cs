using ChoicePie.Backend.Application.GameRooms.Dtos;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public sealed class JoinRoomCommandHandler(IGameRoomRepository gameRoomRepository)
    : IRequestHandler<JoinRoomCommand, JoinRoomResult>
{
    public async Task<JoinRoomResult> Handle(JoinRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await gameRoomRepository.GetByRoomCodeAsync(request.RoomCode, cancellationToken)
                   ?? throw new RoomNotFoundException(request.RoomCode);

        var player = room.Join(request.Nickname, request.ConnectionId, DateTime.UtcNow, request.MemberId);

        await gameRoomRepository.SaveAsync(room, cancellationToken);

        var playerDto = PlayerDto.ForPublic(player, rank: room.Players.Count, room.CurrentQuestionIndex);

        var players = room.Players
            .OrderByDescending(p => p.Score)
            .Select((p, index) => PlayerDto.ForPublic(p, index + 1, room.CurrentQuestionIndex))
            .ToList();

        var roomDto = new GameRoomDto(
            room.RoomCode,
            GameRoomDto.StatusOf(room.Phase),
            players,
            room.CurrentQuestionIndex,
            room.Questions.Count);

        var roomState = new RoomStateSyncDto(room.Phase.Name, roomDto, null, null, null, null, null);

        return new JoinRoomResult(playerDto, roomState);
    }
}

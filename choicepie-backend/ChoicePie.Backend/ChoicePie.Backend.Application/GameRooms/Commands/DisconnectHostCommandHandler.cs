using ChoicePie.Backend.Application.GameRooms.Dtos;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Enums;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public sealed class DisconnectHostCommandHandler(
    IGameRoomRepository gameRoomRepository,
    TimeProvider timeProvider)
    : IRequestHandler<DisconnectHostCommand, HostDisconnectResultDto>
{
    public async Task<HostDisconnectResultDto> Handle(DisconnectHostCommand request, CancellationToken cancellationToken)
    {
        // 房間可能已過期被清除：沒有東西可更新，視為忽略。
        var room = await gameRoomRepository.GetByRoomCodeAsync(request.RoomCode, cancellationToken);
        if (room is null)
        {
            return new HostDisconnectResultDto(HostDisconnectOutcome.Ignored, null);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var outcome = room.DisconnectHost(request.ConnectionId, now);

        if (outcome == HostDisconnectOutcome.Ignored)
        {
            return new HostDisconnectResultDto(outcome, null);
        }

        await gameRoomRepository.SaveAsync(room, cancellationToken);

        var pause = outcome == HostDisconnectOutcome.MarkedOfflineAndPaused
            ? new PauseStateDto(room.IsPaused, room.GetRemainingMs(now))
            : null;

        return new HostDisconnectResultDto(outcome, pause);
    }
}

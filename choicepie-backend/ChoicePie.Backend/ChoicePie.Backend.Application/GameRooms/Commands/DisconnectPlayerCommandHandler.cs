using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Enums;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public sealed class DisconnectPlayerCommandHandler(IGameRoomRepository gameRoomRepository, GameRoomLock roomLock)
    : IRequestHandler<DisconnectPlayerCommand, PlayerDisconnectOutcome>
{
    public async Task<PlayerDisconnectOutcome> Handle(DisconnectPlayerCommand request, CancellationToken cancellationToken)
    {
        using var _ = await roomLock.AcquireAsync(request.RoomCode, cancellationToken);

        // 房間可能已過期被清除：此時沒有東西可更新，視為忽略即可。
        var room = await gameRoomRepository.GetByRoomCodeAsync(request.RoomCode, cancellationToken);
        if (room is null)
        {
            return PlayerDisconnectOutcome.Ignored;
        }

        var outcome = room.DisconnectPlayer(request.PlayerId, request.ConnectionId);

        if (outcome != PlayerDisconnectOutcome.Ignored)
        {
            await gameRoomRepository.SaveAsync(room, cancellationToken);
        }

        return outcome;
    }
}

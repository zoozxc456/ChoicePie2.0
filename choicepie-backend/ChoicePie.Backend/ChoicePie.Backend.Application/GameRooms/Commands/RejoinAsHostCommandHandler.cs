using ChoicePie.Backend.Application.GameRooms.Dtos;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public sealed class RejoinAsHostCommandHandler(
    IGameRoomRepository gameRoomRepository,
    GameRoomLock roomLock,
    TimeProvider timeProvider)
    : IRequestHandler<RejoinAsHostCommand, RoomStateSyncDto>
{
    public async Task<RoomStateSyncDto> Handle(RejoinAsHostCommand request, CancellationToken cancellationToken)
    {
        using var _ = await roomLock.AcquireAsync(request.RoomCode, cancellationToken);

        var room = await gameRoomRepository.GetByRoomCodeAsync(request.RoomCode, cancellationToken)
                   ?? throw new RoomNotFoundException(request.RoomCode);

        room.AttachHostConnection(request.HostUserId, request.ConnectionId);

        await gameRoomRepository.SaveAsync(room, cancellationToken);

        return GameRoomStateFactory.Build(room, timeProvider.GetUtcNow().UtcDateTime, forHost: true);
    }
}

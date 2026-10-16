using ChoicePie.Backend.Application.GameRooms.Dtos;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Queries;

public sealed class RejoinRoomQueryHandler(IGameRoomRepository gameRoomRepository, TimeProvider timeProvider)
    : IRequestHandler<RejoinRoomQuery, RoomStateSyncDto>
{
    public async Task<RoomStateSyncDto> Handle(RejoinRoomQuery request, CancellationToken cancellationToken)
    {
        var room = await gameRoomRepository.GetByRoomCodeAsync(request.RoomCode, cancellationToken)
                   ?? throw new RoomNotFoundException(request.RoomCode);

        room.EnsureHost(request.HostUserId);

        return GameRoomStateFactory.Build(room, timeProvider.GetUtcNow().UtcDateTime, forHost: true);
    }
}

using ChoicePie.Backend.Application.GameRooms.Dtos;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Queries;

public sealed class WatchRoomQueryHandler(IGameRoomRepository gameRoomRepository, TimeProvider timeProvider)
    : IRequestHandler<WatchRoomQuery, RoomStateSyncDto>
{
    public async Task<RoomStateSyncDto> Handle(WatchRoomQuery request, CancellationToken cancellationToken)
    {
        var room = await gameRoomRepository.GetByRoomCodeAsync(request.RoomCode, cancellationToken)
                   ?? throw new RoomNotFoundException(request.RoomCode);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        room.EnsureViewable(now);

        return GameRoomStateFactory.Build(room, now, forHost: false);
    }
}

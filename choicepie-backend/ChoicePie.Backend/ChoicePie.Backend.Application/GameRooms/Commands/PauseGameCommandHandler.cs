using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public sealed class PauseGameCommandHandler(IGameRoomRepository gameRoomRepository)
    : IRequestHandler<PauseGameCommand, bool>
{
    public async Task<bool> Handle(PauseGameCommand request, CancellationToken cancellationToken)
    {
        var room = await gameRoomRepository.GetByRoomCodeAsync(request.RoomCode, cancellationToken)
                   ?? throw new RoomNotFoundException(request.RoomCode);

        room.TogglePause(request.HostUserId, DateTime.UtcNow);

        await gameRoomRepository.SaveAsync(room, cancellationToken);

        return room.PausedAtUtc is not null;
    }
}

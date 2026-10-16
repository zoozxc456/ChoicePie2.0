using ChoicePie.Backend.Application.GameRooms.Dtos;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public sealed class PauseGameCommandHandler(IGameRoomRepository gameRoomRepository, TimeProvider timeProvider)
    : IRequestHandler<PauseGameCommand, PauseStateDto>
{
    public async Task<PauseStateDto> Handle(PauseGameCommand request, CancellationToken cancellationToken)
    {
        var room = await gameRoomRepository.GetByRoomCodeAsync(request.RoomCode, cancellationToken)
                   ?? throw new RoomNotFoundException(request.RoomCode);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        room.TogglePause(request.HostUserId, now);

        await gameRoomRepository.SaveAsync(room, cancellationToken);

        return new PauseStateDto(room.IsPaused, room.GetRemainingMs(now));
    }
}

using ChoicePie.Backend.Application.GameRooms.Dtos;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public sealed class StartGameCommandHandler(IGameRoomRepository gameRoomRepository)
    : IRequestHandler<StartGameCommand, QuestionPayloadDto>
{
    public async Task<QuestionPayloadDto> Handle(StartGameCommand request, CancellationToken cancellationToken)
    {
        var room = await gameRoomRepository.GetByRoomCodeAsync(request.RoomCode, cancellationToken)
                   ?? throw new RoomNotFoundException(request.RoomCode);

        GameRoomCommandGuards.EnsureHost(room, request.HostUserId);

        room.StartGame(DateTime.UtcNow);

        await gameRoomRepository.SaveAsync(room, cancellationToken);

        return GameRoomCommandGuards.CurrentQuestionPayload(room);
    }
}

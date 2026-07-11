using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public sealed record CreateRoomResultDto(string RoomCode);

public sealed class CreateRoomCommand : IRequest<CreateRoomResultDto>
{
    public required Guid HostUserId { get; init; }
    public required Guid QuizId { get; init; }
    public IReadOnlyList<Guid> QuestionIds { get; init; } = [];
    public int TimeLimitSeconds { get; init; } = 20;
}

using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public sealed record PauseGameCommand(string RoomCode, Guid HostUserId) : IRequest<bool>;

using ChoicePie.Backend.Domain.Aggregates.GameRoom.Enums;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public sealed record DisconnectPlayerCommand(string RoomCode, Guid PlayerId, string ConnectionId)
    : IRequest<PlayerDisconnectOutcome>;

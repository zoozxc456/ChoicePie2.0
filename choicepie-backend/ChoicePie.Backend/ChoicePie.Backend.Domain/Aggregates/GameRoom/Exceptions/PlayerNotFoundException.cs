using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;

public sealed class PlayerNotFoundException(Guid playerId, string roomCode)
    : DomainException(
        internalLogMessage: $"Player {playerId} not found in room {roomCode}.",
        presentationMessage: "找不到此玩家。",
        errorCode: "GAME_ROOM_PLAYER_NOT_FOUND",
        statusCode: HttpStatusCode.NotFound);

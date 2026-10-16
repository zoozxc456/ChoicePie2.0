using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;

public sealed class InvalidReconnectTokenException(string roomCode)
    : DomainException(
        internalLogMessage: $"Invalid reconnect token for room {roomCode}.",
        presentationMessage: "無法重新連線，找不到對應的玩家。",
        errorCode: "GAME_ROOM_INVALID_RECONNECT_TOKEN",
        statusCode: HttpStatusCode.NotFound);

using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;

public sealed class RoomAccessDeniedException(string roomCode)
    : DomainException(
        internalLogMessage: $"Caller is not the host of room {roomCode}.",
        presentationMessage: "您不是此房間的主持人。",
        errorCode: "GAME_ROOM_ACCESS_DENIED",
        statusCode: HttpStatusCode.Forbidden);

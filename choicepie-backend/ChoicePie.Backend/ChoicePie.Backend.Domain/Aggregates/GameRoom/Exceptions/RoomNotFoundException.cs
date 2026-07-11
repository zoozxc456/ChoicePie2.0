using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;

public sealed class RoomNotFoundException(string roomCode)
    : DomainException(
        internalLogMessage: $"Room {roomCode} not found.",
        presentationMessage: "找不到此房間，可能已過期或不存在。",
        errorCode: "GAME_ROOM_NOT_FOUND",
        statusCode: HttpStatusCode.NotFound);

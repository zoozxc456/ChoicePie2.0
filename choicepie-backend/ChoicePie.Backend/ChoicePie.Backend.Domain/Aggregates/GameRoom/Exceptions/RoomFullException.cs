using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;

public sealed class RoomFullException(string roomCode)
    : DomainException(
        internalLogMessage: $"Room {roomCode} is full.",
        presentationMessage: "房間人數已達上限（30人）。",
        errorCode: "GAME_ROOM_FULL",
        statusCode: HttpStatusCode.Conflict);

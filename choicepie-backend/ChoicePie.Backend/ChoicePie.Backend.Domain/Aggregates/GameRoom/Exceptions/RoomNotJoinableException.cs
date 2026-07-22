using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;

public sealed class RoomNotJoinableException(string roomCode)
    : DomainException(
        internalLogMessage: $"Room {roomCode} is not joinable (expired or read-only).",
        presentationMessage: "此房間已無法加入。",
        errorCode: "GAME_ROOM_NOT_JOINABLE",
        statusCode: HttpStatusCode.Conflict);

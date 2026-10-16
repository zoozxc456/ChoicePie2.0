using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;

public sealed class GamePausedException(string roomCode)
    : DomainException(
        internalLogMessage: $"Room {roomCode} is paused; answers are rejected.",
        presentationMessage: "遊戲暫停中，暫時無法作答。",
        errorCode: "GAME_ROOM_PAUSED",
        statusCode: HttpStatusCode.Conflict);

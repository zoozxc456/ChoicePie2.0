using System.Net;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Enums;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;

public sealed class InvalidGamePhaseException(GamePhase actual, GamePhase expected)
    : DomainException(
        internalLogMessage: $"Game room is in phase {actual.Name} but {expected.Name} was required.",
        presentationMessage: "遊戲目前狀態不允許此操作。",
        errorCode: "GAME_ROOM_INVALID_PHASE",
        statusCode: HttpStatusCode.Conflict);

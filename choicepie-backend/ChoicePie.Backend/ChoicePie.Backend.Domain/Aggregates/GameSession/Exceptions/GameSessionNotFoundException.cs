using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.GameSession.Exceptions;

public sealed class GameSessionNotFoundException(Guid sessionId)
    : DomainException(
        internalLogMessage: $"GameSession {sessionId} not found.",
        presentationMessage: "找不到指定的遊戲紀錄。",
        errorCode: "GAME_SESSION_NOT_FOUND",
        statusCode: HttpStatusCode.NotFound);

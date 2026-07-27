using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.GameSession.Exceptions;

public sealed class GameSessionForbiddenException(Guid sessionId, Guid userId)
    : DomainException(
        internalLogMessage: $"User {userId} is not allowed to access game session {sessionId}.",
        presentationMessage: "您沒有權限存取此遊戲紀錄。",
        errorCode: "GAME_SESSION_FORBIDDEN",
        statusCode: HttpStatusCode.Forbidden);

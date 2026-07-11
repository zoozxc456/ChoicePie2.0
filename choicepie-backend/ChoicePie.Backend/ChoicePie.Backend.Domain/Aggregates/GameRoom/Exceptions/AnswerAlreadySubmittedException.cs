using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;

public sealed class AnswerAlreadySubmittedException(Guid playerId, int questionIndex)
    : DomainException(
        internalLogMessage: $"Player {playerId} already answered question {questionIndex}.",
        presentationMessage: "已經作答過這一題。",
        errorCode: "GAME_ROOM_ANSWER_ALREADY_SUBMITTED",
        statusCode: HttpStatusCode.Conflict);

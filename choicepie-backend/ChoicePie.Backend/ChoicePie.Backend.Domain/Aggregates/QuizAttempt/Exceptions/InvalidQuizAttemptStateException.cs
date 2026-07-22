using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Exceptions;

public sealed class InvalidQuizAttemptStateException(Guid attemptId, string reason)
    : DomainException(
        internalLogMessage: $"QuizAttempt {attemptId} is in an invalid state for this operation: {reason}",
        presentationMessage: reason,
        errorCode: "INVALID_QUIZ_ATTEMPT_STATE",
        statusCode: HttpStatusCode.BadRequest);

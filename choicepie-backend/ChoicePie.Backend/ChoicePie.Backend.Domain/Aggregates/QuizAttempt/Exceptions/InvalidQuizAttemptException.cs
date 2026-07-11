using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Exceptions;

public sealed class InvalidQuizAttemptException(string reason)
    : DomainException(
        internalLogMessage: $"Invalid quiz attempt: {reason}",
        presentationMessage: reason,
        errorCode: "INVALID_QUIZ_ATTEMPT",
        statusCode: HttpStatusCode.BadRequest);

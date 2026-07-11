using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Exceptions;

public sealed class QuizAttemptNotFoundException(Guid attemptId)
    : DomainException(
        internalLogMessage: $"QuizAttempt {attemptId} not found.",
        presentationMessage: "找不到指定的作答紀錄。",
        errorCode: "QUIZ_ATTEMPT_NOT_FOUND",
        statusCode: HttpStatusCode.NotFound);

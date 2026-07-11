using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Exceptions;

public sealed class QuizAttemptForbiddenException(Guid attemptId, Guid memberId)
    : DomainException(
        internalLogMessage: $"Member {memberId} is not allowed to access quiz attempt {attemptId}.",
        presentationMessage: "您沒有權限存取此作答紀錄。",
        errorCode: "QUIZ_ATTEMPT_FORBIDDEN",
        statusCode: HttpStatusCode.Forbidden);

using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;

public sealed class QuizForbiddenException(Guid quizId, Guid userId)
    : DomainException(
        internalLogMessage: $"User {userId} is not allowed to modify quiz {quizId}.",
        presentationMessage: "您沒有權限修改此題庫。",
        errorCode: "QUIZ_FORBIDDEN",
        statusCode: HttpStatusCode.Forbidden);

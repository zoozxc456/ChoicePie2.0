using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;

public sealed class QuizNotPublishedException(Guid quizId)
    : DomainException(
        internalLogMessage: $"Quiz {quizId} is not published, cannot be attempted.",
        presentationMessage: "此題庫尚未發布，無法作答。",
        errorCode: "QUIZ_NOT_PUBLISHED",
        statusCode: HttpStatusCode.BadRequest);

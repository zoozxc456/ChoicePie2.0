using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;

public sealed class QuizNotFoundException(Guid quizId)
    : DomainException(
        internalLogMessage: $"Quiz {quizId} not found.",
        presentationMessage: "找不到指定的題庫。",
        errorCode: "QUIZ_NOT_FOUND",
        statusCode: HttpStatusCode.NotFound);

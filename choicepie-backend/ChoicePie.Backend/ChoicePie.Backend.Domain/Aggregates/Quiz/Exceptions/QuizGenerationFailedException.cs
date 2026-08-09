using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;

public sealed class QuizGenerationFailedException(string reason, Exception? innerException = null)
    : DomainException(
        internalLogMessage: $"AI quiz question generation failed: {reason}",
        presentationMessage: "AI 出題失敗，請稍後再試。",
        errorCode: "QUIZ_GENERATION_FAILED",
        statusCode: HttpStatusCode.BadGateway,
        innerException: innerException);

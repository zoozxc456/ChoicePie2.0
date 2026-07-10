using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;

public sealed class AiGenerationQuotaExceededException(Guid userId)
    : DomainException(
        internalLogMessage: $"User {userId} exceeded the daily AI quiz-generation quota.",
        presentationMessage: "今日 AI 出題次數已用完，請明天再試。",
        errorCode: "AI_GENERATION_QUOTA_EXCEEDED",
        statusCode: HttpStatusCode.TooManyRequests);

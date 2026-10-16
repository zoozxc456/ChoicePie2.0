using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;

public sealed class AiTokenBudgetExceededException(Guid userId)
    : DomainException(
        internalLogMessage: $"User {userId} exceeded the daily AI token budget.",
        presentationMessage: "今日 AI 出題 token 用量已超過上限，請明天再試。",
        errorCode: "AI_TOKEN_BUDGET_EXCEEDED",
        statusCode: HttpStatusCode.TooManyRequests);

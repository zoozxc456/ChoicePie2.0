using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;

/// <summary>
/// 會員未指派任何方案：未指派方案視同 AI 額度為 0，與「今日額度用完」是不同情況（明天也不會恢復）。
/// </summary>
public sealed class AiGenerationNotAvailableException(Guid userId)
    : DomainException(
        internalLogMessage: $"User {userId} has no membership tier assigned; AI quiz generation is unavailable.",
        presentationMessage: "目前的方案不包含 AI 出題功能。",
        errorCode: "AI_GENERATION_NOT_AVAILABLE",
        statusCode: HttpStatusCode.Forbidden);

using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;

public sealed class AiGenerationInProgressException(Guid userId)
    : DomainException(
        internalLogMessage: $"User {userId} already has an AI quiz generation in progress.",
        presentationMessage: "AI 正在為你出題中，請等這次完成後再試。",
        errorCode: "AI_GENERATION_IN_PROGRESS",
        statusCode: HttpStatusCode.Conflict);

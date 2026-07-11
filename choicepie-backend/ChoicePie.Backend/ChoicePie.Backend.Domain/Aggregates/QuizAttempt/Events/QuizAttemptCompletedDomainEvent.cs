using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Events;

public sealed record QuizAttemptCompletedDomainEvent(
    Guid AttemptId,
    Guid QuizId,
    Guid MemberId,
    decimal Score,
    bool Passed) : BaseDomainEvent;

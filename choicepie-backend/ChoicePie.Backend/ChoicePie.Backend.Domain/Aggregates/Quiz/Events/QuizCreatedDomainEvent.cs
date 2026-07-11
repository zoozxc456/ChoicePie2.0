using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Events;

public sealed record QuizCreatedDomainEvent(Guid QuizId, Guid CreatorId, string Title) : BaseDomainEvent;

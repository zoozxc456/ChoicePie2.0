using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Events;
using ChoicePie.Backend.Shared.Application.Events;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.EventHandlers;

// Delivered through the outbox, so the outcome is recorded even if the node dies right after the
// attempt commits. Delivery is at-least-once: a crash between this UPDATE and the outbox marking the
// handler completed can count one attempt twice - acceptable for a statistic, so it is not deduplicated.
// The repository applies the outcome as one atomic UPDATE so concurrent completions are never lost.
public sealed class RecordChallengeOutcomeHandler(IQuizRepository quizRepository)
    : INotificationHandler<ReliableDomainEventNotification<QuizAttemptCompletedDomainEvent>>
{
    public Task Handle(
        ReliableDomainEventNotification<QuizAttemptCompletedDomainEvent> notification, CancellationToken cancellationToken) =>
        quizRepository.RecordChallengeOutcomeAsync(
            notification.DomainEvent.QuizId, notification.DomainEvent.Passed, cancellationToken);
}

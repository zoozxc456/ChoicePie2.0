using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Events;
using ChoicePie.Backend.Shared.Application.Events;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.EventHandlers;

// EfUnitOfWork dispatches domain events after commit, outside any transaction, so delivery is
// at-least-once, not exactly-once - this handler is not deduplicated (best effort).
// The repository applies the outcome as one atomic UPDATE so concurrent completions are never lost.
public sealed class RecordChallengeOutcomeHandler(IQuizRepository quizRepository)
    : INotificationHandler<DomainEventNotification<QuizAttemptCompletedDomainEvent>>
{
    public Task Handle(
        DomainEventNotification<QuizAttemptCompletedDomainEvent> notification, CancellationToken cancellationToken) =>
        quizRepository.RecordChallengeOutcomeAsync(
            notification.DomainEvent.QuizId, notification.DomainEvent.Passed, cancellationToken);
}

using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Events;
using ChoicePie.Backend.Shared.Application.Events;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.EventHandlers;

// EfUnitOfWork dispatches domain events after commit, outside any transaction, so delivery is
// at-least-once, not exactly-once - this handler is not deduplicated (best effort).
public sealed class RecordChallengeOutcomeHandler(IQuizRepository quizRepository, IUnitOfWork unitOfWork)
    : INotificationHandler<DomainEventNotification<QuizAttemptCompletedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<QuizAttemptCompletedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var quiz = await quizRepository.GetByIdAsync(notification.DomainEvent.QuizId, cancellationToken);
        if (quiz is null)
        {
            return;
        }

        quiz.RecordChallengeOutcome(notification.DomainEvent.Passed);

        await quizRepository.UpdateAsync(quiz, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

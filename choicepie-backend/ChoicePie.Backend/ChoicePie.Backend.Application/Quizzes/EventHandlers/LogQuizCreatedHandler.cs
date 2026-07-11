using ChoicePie.Backend.Domain.Aggregates.Quiz.Events;
using ChoicePie.Backend.Shared.Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ChoicePie.Backend.Application.Quizzes.EventHandlers;

public sealed class LogQuizCreatedHandler(ILogger<LogQuizCreatedHandler> logger)
    : INotificationHandler<DomainEventNotification<QuizCreatedDomainEvent>>
{
    public Task Handle(DomainEventNotification<QuizCreatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "New quiz created: {QuizId} ({Title}) by {CreatorId}",
            notification.DomainEvent.QuizId,
            notification.DomainEvent.Title,
            notification.DomainEvent.CreatorId);

        return Task.CompletedTask;
    }
}

using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Events;
using ChoicePie.Backend.Shared.Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ChoicePie.Backend.Application.Identity.EventHandlers;

public sealed class LogAuthAccountRegisteredHandler(ILogger<LogAuthAccountRegisteredHandler> logger)
    : INotificationHandler<DomainEventNotification<AuthAccountRegisteredDomainEvent>>
{
    public Task Handle(DomainEventNotification<AuthAccountRegisteredDomainEvent> notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "New auth account registered: {AuthAccountId} for member {MemberId} ({Email})",
            notification.DomainEvent.AuthAccountId,
            notification.DomainEvent.MemberId,
            notification.DomainEvent.Email);

        return Task.CompletedTask;
    }
}

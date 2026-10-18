using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Events;
using ChoicePie.Backend.Shared.Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ChoicePie.Backend.Application.AdminUsers.EventHandlers;

public sealed class LogAdminAuthAccountCreatedHandler(ILogger<LogAdminAuthAccountCreatedHandler> logger)
    : INotificationHandler<DomainEventNotification<AdminAuthAccountCreatedDomainEvent>>
{
    public Task Handle(DomainEventNotification<AdminAuthAccountCreatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "New admin auth account created: {AdminAuthAccountId} for admin user {AdminUserId}",
            notification.DomainEvent.AdminAuthAccountId,
            notification.DomainEvent.AdminUserId);

        return Task.CompletedTask;
    }
}

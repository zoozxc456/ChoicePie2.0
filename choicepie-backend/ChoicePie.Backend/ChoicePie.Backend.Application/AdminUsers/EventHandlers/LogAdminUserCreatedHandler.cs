using ChoicePie.Backend.Domain.Aggregates.AdminUser.Events;
using ChoicePie.Backend.Shared.Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ChoicePie.Backend.Application.AdminUsers.EventHandlers;

public sealed class LogAdminUserCreatedHandler(ILogger<LogAdminUserCreatedHandler> logger)
    : INotificationHandler<DomainEventNotification<AdminUserCreatedDomainEvent>>
{
    public Task Handle(DomainEventNotification<AdminUserCreatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "New admin user created: {AdminUserId} ({Name}, {Role})",
            notification.DomainEvent.AdminUserId,
            notification.DomainEvent.Name,
            notification.DomainEvent.Role);

        return Task.CompletedTask;
    }
}

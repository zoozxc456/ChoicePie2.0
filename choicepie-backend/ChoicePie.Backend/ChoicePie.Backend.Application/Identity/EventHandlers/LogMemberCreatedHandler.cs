using ChoicePie.Backend.Domain.Aggregates.Member.Events;
using ChoicePie.Backend.Shared.Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ChoicePie.Backend.Application.Identity.EventHandlers;

public sealed class LogMemberCreatedHandler(ILogger<LogMemberCreatedHandler> logger)
    : INotificationHandler<DomainEventNotification<MemberCreatedDomainEvent>>
{
    public Task Handle(DomainEventNotification<MemberCreatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "New member created: {MemberId} ({Name})",
            notification.DomainEvent.MemberId,
            notification.DomainEvent.Name);

        return Task.CompletedTask;
    }
}

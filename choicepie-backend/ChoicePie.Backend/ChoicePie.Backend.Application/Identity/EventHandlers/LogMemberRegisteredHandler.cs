using ChoicePie.Backend.Domain.Aggregates.Member.Events;
using ChoicePie.Backend.Shared.Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ChoicePie.Backend.Application.Identity.EventHandlers;

public sealed class LogMemberRegisteredHandler(ILogger<LogMemberRegisteredHandler> logger)
    : INotificationHandler<DomainEventNotification<MemberRegisteredDomainEvent>>
{
    public Task Handle(DomainEventNotification<MemberRegisteredDomainEvent> notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "New member registered: {MemberId} ({Email})",
            notification.DomainEvent.MemberId,
            notification.DomainEvent.Email);

        return Task.CompletedTask;
    }
}

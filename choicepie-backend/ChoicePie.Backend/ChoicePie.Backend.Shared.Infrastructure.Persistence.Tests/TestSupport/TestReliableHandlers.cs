using ChoicePie.Backend.Shared.Application.Events;
using MediatR;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests.TestSupport;

public sealed class SendTestSecretHandler
    : INotificationHandler<ReliableDomainEventNotification<TestSecretIssuedDomainEvent>>
{
    public Task Handle(ReliableDomainEventNotification<TestSecretIssuedDomainEvent> notification,
        CancellationToken cancellationToken) => Task.CompletedTask;
}

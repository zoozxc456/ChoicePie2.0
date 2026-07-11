using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;
using MediatR;

namespace ChoicePie.Backend.Shared.Application.Events;

public class DomainEventNotification<TDomainEvent>(TDomainEvent domainEvent)
    : INotification where TDomainEvent : BaseDomainEvent
{
    public TDomainEvent DomainEvent { get; } = domainEvent;
}
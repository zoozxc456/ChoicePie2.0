using System.ComponentModel.DataAnnotations.Schema;
using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

public abstract class AggregateRoot<TId> : AuditableEntity<TId>, IHasDomainEvents
{
    [NotMapped] private readonly List<BaseDomainEvent> _domainEvents = [];
    [NotMapped] public IReadOnlyCollection<BaseDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(BaseDomainEvent baseDomainEvent)
    {
        _domainEvents.Add(baseDomainEvent);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
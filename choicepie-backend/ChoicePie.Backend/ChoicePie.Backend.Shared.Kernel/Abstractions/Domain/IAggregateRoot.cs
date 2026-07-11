namespace ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

public interface IAggregateRoot
{
    void AddDomainEvent(BaseDomainEvent baseDomainEvent);
    void ClearDomainEvents();
}
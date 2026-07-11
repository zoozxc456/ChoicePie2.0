namespace ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

public abstract record BaseDomainEvent
{
    DateTime OccurredOn { get; } = DateTime.UtcNow;
}
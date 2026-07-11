namespace ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

public interface IAggregateRootBuilder<out TAggregateRoot>
{
    TAggregateRoot Build();
}
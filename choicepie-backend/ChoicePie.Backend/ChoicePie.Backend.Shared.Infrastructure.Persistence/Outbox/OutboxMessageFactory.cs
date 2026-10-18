using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessageFactory(OutboxSerializer serializer, OutboxEventRegistry registry)
{
    public IEnumerable<OutboxMessage> CreateFor(IEnumerable<BaseDomainEvent> domainEvents) =>
        domainEvents
            .Where(e => registry.RequiresOutbox(e.GetType()))
            .Select(e =>
            {
                var (type, payload) = serializer.Serialize(e);
                return OutboxMessage.Create(e.EventId, type, payload, e.OccurredOn);
            });
}

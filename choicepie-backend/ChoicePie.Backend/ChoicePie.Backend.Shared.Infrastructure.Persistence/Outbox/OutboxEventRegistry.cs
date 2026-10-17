using System.Reflection;
using ChoicePie.Backend.Shared.Application.Events;
using MediatR;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Outbox;

/// <summary>
/// 哪些事件型別有可靠 handler（<see cref="ReliableDomainEventNotification{TDomainEvent}"/>）。
/// 只有這些事件會寫進 Outbox；沒人可靠訂閱的事件不寫，免得白白序列化、加密、再刪掉。
/// </summary>
public sealed class OutboxEventRegistry(IReadOnlySet<Type> eventTypes)
{
    public IReadOnlySet<Type> EventTypes { get; } = eventTypes;

    public bool RequiresOutbox(Type eventType) => EventTypes.Contains(eventType);

    public static OutboxEventRegistry FromAssemblies(params Assembly[] assemblies)
    {
        var eventTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces())
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
            .Select(i => i.GetGenericArguments()[0])
            .Where(n => n.IsGenericType && n.GetGenericTypeDefinition() == typeof(ReliableDomainEventNotification<>))
            .Select(n => n.GetGenericArguments()[0])
            .ToHashSet();

        return new OutboxEventRegistry(eventTypes);
    }
}

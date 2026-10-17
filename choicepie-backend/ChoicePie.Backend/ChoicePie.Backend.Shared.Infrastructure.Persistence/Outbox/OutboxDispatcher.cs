using ChoicePie.Backend.Shared.Application.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Outbox;

public sealed record OutboxDispatchResult(IReadOnlyList<string> CompletedHandlers, string? Error)
{
    public bool Succeeded => Error is null;
}

/// <summary>
/// 把一筆 outbox 事件送給它的可靠 handler。不走 MediatR 的 Publish：Publish 碰到第一個例外就停，
/// 後面的 handler 都不會跑。這裡逐一呼叫、各自一個 DI scope（各自的 DbContext），
/// 並回報哪些成功，讓重試只補跑失敗的那幾個。
/// </summary>
public sealed class OutboxDispatcher(IServiceScopeFactory scopeFactory, OutboxSerializer serializer)
{
    public async Task<OutboxDispatchResult> DispatchAsync(OutboxMessage message, CancellationToken ct)
    {
        var domainEvent = serializer.Deserialize(message.Type, message.Payload);
        var notificationType = typeof(ReliableDomainEventNotification<>).MakeGenericType(domainEvent.GetType());
        var handlerInterface = typeof(INotificationHandler<>).MakeGenericType(notificationType);
        var notification = Activator.CreateInstance(notificationType, domainEvent)!;

        var handlerTypes = ResolveHandlerTypes(handlerInterface);
        var completed = new List<string>();
        var errors = new List<string>();

        foreach (var handlerType in handlerTypes)
        {
            var handlerName = HandlerNameOf(handlerType);
            if (message.CompletedHandlers.Contains(handlerName))
            {
                continue;
            }

            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var handler = scope.ServiceProvider.GetServices(handlerInterface)
                    .First(h => h!.GetType() == handlerType)!;
                await (Task)handlerInterface.GetMethod(nameof(INotificationHandler<INotification>.Handle))!
                    .Invoke(handler, [notification, ct])!;
                completed.Add(handlerName);
            }
            catch (Exception ex)
            {
                errors.Add($"{handlerName}: {ex.GetBaseException().Message}");
            }
        }

        return new OutboxDispatchResult(completed, errors.Count == 0 ? null : string.Join("\n", errors));
    }

    public static string HandlerNameOf(Type handlerType) => handlerType.FullName!;

    private List<Type> ResolveHandlerTypes(Type handlerInterface)
    {
        using var scope = scopeFactory.CreateScope();
        return scope.ServiceProvider.GetServices(handlerInterface).Select(h => h!.GetType()).ToList();
    }
}

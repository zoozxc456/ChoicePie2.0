using ChoicePie.Backend.Shared.Application.Events;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Outbox;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests.TestSupport;
using MediatR;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests.Outbox;

[TestFixture]
public class OutboxDispatcherTests
{
    private readonly OutboxSerializer _serializer = new(new EphemeralDataProtectionProvider());
    private HandlerCallLog _log = null!;

    [SetUp]
    public void SetUp() => _log = new HandlerCallLog();

    [Test]
    public async Task DispatchAsync_GivenAllHandlersSucceed_WhenDispatched_ThenEachReceivesTheRestoredEventAndResultSucceeds()
    {
        var dispatcher = CreateDispatcher(typeof(FirstRecordingHandler), typeof(SecondRecordingHandler));
        var domainEvent = new TestSecretIssuedDomainEvent(Guid.NewGuid(), "token");

        var result = await dispatcher.DispatchAsync(ToMessage(domainEvent), CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(_log.Calls, Is.EquivalentTo(new[]
        {
            (nameof(FirstRecordingHandler), domainEvent),
            (nameof(SecondRecordingHandler), domainEvent)
        }));
    }

    [Test]
    public async Task DispatchAsync_GivenOneHandlerThrows_WhenDispatched_ThenOtherHandlersStillRunAndOnlyTheyAreMarkedCompleted()
    {
        var dispatcher = CreateDispatcher(typeof(ThrowingHandler), typeof(FirstRecordingHandler));

        var result = await dispatcher.DispatchAsync(
            ToMessage(new TestSecretIssuedDomainEvent(Guid.NewGuid(), "token")), CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Error, Does.Contain("boom"));
        Assert.That(result.CompletedHandlers, Is.EqualTo(new[] { OutboxDispatcher.HandlerNameOf(typeof(FirstRecordingHandler)) }));
        Assert.That(_log.Calls.Select(c => c.Handler), Is.EqualTo(new[] { nameof(FirstRecordingHandler) }));
    }

    // 重試時只補跑上次失敗的 handler：已經寄出的信、已經更新的資料不再做第二次。
    [Test]
    public async Task DispatchAsync_GivenHandlerCompletedInEarlierAttempt_WhenRetried_ThenItIsNotInvokedAgain()
    {
        var dispatcher = CreateDispatcher(typeof(FirstRecordingHandler), typeof(SecondRecordingHandler));
        var message = ToMessage(new TestSecretIssuedDomainEvent(Guid.NewGuid(), "token"));
        message.MarkHandlersCompleted([OutboxDispatcher.HandlerNameOf(typeof(FirstRecordingHandler))]);

        var result = await dispatcher.DispatchAsync(message, CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(_log.Calls.Select(c => c.Handler), Is.EqualTo(new[] { nameof(SecondRecordingHandler) }));
    }

    private OutboxDispatcher CreateDispatcher(params Type[] handlerTypes)
    {
        var services = new ServiceCollection().AddSingleton(_log);
        foreach (var handlerType in handlerTypes)
        {
            services.AddScoped(
                typeof(INotificationHandler<ReliableDomainEventNotification<TestSecretIssuedDomainEvent>>), handlerType);
        }

        return new OutboxDispatcher(services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>(), _serializer);
    }

    private OutboxMessage ToMessage(TestSecretIssuedDomainEvent domainEvent)
    {
        var (type, payload) = _serializer.Serialize(domainEvent);
        return OutboxMessage.Create(domainEvent.EventId, type, payload, domainEvent.OccurredOn);
    }

    public sealed class HandlerCallLog
    {
        public List<(string Handler, TestSecretIssuedDomainEvent Event)> Calls { get; } = [];
    }

    private sealed class FirstRecordingHandler(HandlerCallLog log)
        : INotificationHandler<ReliableDomainEventNotification<TestSecretIssuedDomainEvent>>
    {
        public Task Handle(ReliableDomainEventNotification<TestSecretIssuedDomainEvent> notification, CancellationToken ct)
        {
            log.Calls.Add((nameof(FirstRecordingHandler), notification.DomainEvent));
            return Task.CompletedTask;
        }
    }

    private sealed class SecondRecordingHandler(HandlerCallLog log)
        : INotificationHandler<ReliableDomainEventNotification<TestSecretIssuedDomainEvent>>
    {
        public Task Handle(ReliableDomainEventNotification<TestSecretIssuedDomainEvent> notification, CancellationToken ct)
        {
            log.Calls.Add((nameof(SecondRecordingHandler), notification.DomainEvent));
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingHandler
        : INotificationHandler<ReliableDomainEventNotification<TestSecretIssuedDomainEvent>>
    {
        public Task Handle(ReliableDomainEventNotification<TestSecretIssuedDomainEvent> notification, CancellationToken ct) =>
            throw new InvalidOperationException("boom");
    }
}

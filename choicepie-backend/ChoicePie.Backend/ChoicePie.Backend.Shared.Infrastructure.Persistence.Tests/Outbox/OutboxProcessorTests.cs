using ChoicePie.Backend.Shared.Application.Events;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Extensions;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Outbox;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests.TestSupport;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests.Outbox;

/// <summary>
/// 從 UnitOfWork 寫入到 processor 派送的完整一圈，DI 走正式的 AddSharedPersistence 註冊。
/// </summary>
public sealed class OutboxProcessorTests
{
    private PersistenceTestFixture _fixture = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = new PersistenceTestFixture();
        await _fixture.InitializeAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown() => await _fixture.DisposeAsync();

    [SetUp]
    public async Task SetUp()
    {
        await using var context = _fixture.CreateContext();
        await context.Set<OutboxMessage>().ExecuteDeleteAsync();
    }

    [Test]
    public async Task ProcessBatchAsync_GivenCommittedReliableEvent_WhenProcessed_ThenHandlerReceivesItAndMessageIsRemoved()
    {
        var received = new List<TestSecretIssuedDomainEvent>();
        await using var provider = BuildProvider(services => services.AddSingleton(received)
            .AddScoped<INotificationHandler<ReliableDomainEventNotification<TestSecretIssuedDomainEvent>>, CollectingHandler>());
        var widget = await SaveWidgetWithSecretAsync(provider, "raw-token");

        var processed = await provider.GetRequiredService<OutboxProcessor>().ProcessBatchAsync(CancellationToken.None);

        Assert.That(processed, Is.EqualTo(1));
        Assert.That(received.Single().WidgetId, Is.EqualTo(widget.Id));
        Assert.That(received.Single().RawToken, Is.EqualTo("raw-token"));
        await using var verify = _fixture.CreateContext();
        Assert.That(await verify.Set<OutboxMessage>().AnyAsync(), Is.False);
    }

    [Test]
    public async Task ProcessBatchAsync_GivenHandlerFails_WhenProcessed_ThenMessageStaysQueuedWithTheError()
    {
        await using var provider = BuildProvider(services => services
            .AddScoped<INotificationHandler<ReliableDomainEventNotification<TestSecretIssuedDomainEvent>>, FailingHandler>());
        await SaveWidgetWithSecretAsync(provider, "raw-token");

        await provider.GetRequiredService<OutboxProcessor>().ProcessBatchAsync(CancellationToken.None);

        await using var verify = _fixture.CreateContext();
        var message = await verify.Set<OutboxMessage>().SingleAsync();
        Assert.That(message.AttemptCount, Is.EqualTo(1));
        Assert.That(message.LastError, Does.Contain("smtp down"));
    }

    private ServiceProvider BuildProvider(Action<IServiceCollection> registerHandlers)
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(TimeProvider.System)
            .AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider())
            .AddSingleton(Substitute.For<IPublisher>())
            .AddScoped(_ => _fixture.CreateContext())
            .AddSharedPersistence<TestDbContext>(typeof(TestWidget).Assembly);
        registerHandlers(services);
        return services.BuildServiceProvider();
    }

    private static async Task<TestWidget> SaveWidgetWithSecretAsync(IServiceProvider provider, string rawToken)
    {
        await using var scope = provider.CreateAsyncScope();
        var widget = TestWidget.Create("Processor Widget");
        widget.IssueSecret(rawToken);
        await scope.ServiceProvider.GetRequiredService<TestDbContext>().AddAsync(widget);
        await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync();
        return widget;
    }

    private sealed class CollectingHandler(List<TestSecretIssuedDomainEvent> received)
        : INotificationHandler<ReliableDomainEventNotification<TestSecretIssuedDomainEvent>>
    {
        public Task Handle(ReliableDomainEventNotification<TestSecretIssuedDomainEvent> notification, CancellationToken ct)
        {
            received.Add(notification.DomainEvent);
            return Task.CompletedTask;
        }
    }

    private sealed class FailingHandler
        : INotificationHandler<ReliableDomainEventNotification<TestSecretIssuedDomainEvent>>
    {
        public Task Handle(ReliableDomainEventNotification<TestSecretIssuedDomainEvent> notification, CancellationToken ct) =>
            throw new InvalidOperationException("smtp down");
    }
}

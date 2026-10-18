using ChoicePie.Backend.Shared.Infrastructure.Persistence.Outbox;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests.TestSupport;
using ChoicePie.Backend.Shared.Kernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests.Outbox;

/// <summary>
/// Outbox 的核心保證：事件跟業務資料同生共死。業務資料 commit 了，事件一定在 outbox 裡；
/// 業務資料沒寫進去，outbox 也不能留下一筆孤兒事件。
/// </summary>
public sealed class EfUnitOfWorkOutboxTests
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

    [Test]
    public async Task SaveChangesAsync_GivenEventWithReliableHandler_WhenCommitted_ThenOutboxHoldsTheEvent()
    {
        await using var context = _fixture.CreateContext();
        var widget = TestWidget.Create("Secret Holder");
        widget.IssueSecret("raw-token");
        await context.AddAsync(widget);
        var issued = widget.DomainEvents.OfType<TestSecretIssuedDomainEvent>().Single();

        await _fixture.CreateUnitOfWork(context, Substitute.For<IPublisher>()).SaveChangesAsync();

        await using var verify = _fixture.CreateContext();
        var message = await verify.Set<OutboxMessage>().SingleOrDefaultAsync(m => m.Id == issued.EventId);
        Assert.That(message, Is.Not.Null);
        Assert.That(message!.Type, Is.EqualTo(OutboxSerializer.TypeNameOf(typeof(TestSecretIssuedDomainEvent))));
    }

    [Test]
    public async Task SaveChangesAsync_GivenEventWithOnlyInProcessHandlers_WhenCommitted_ThenNothingIsWrittenToOutbox()
    {
        await using var context = _fixture.CreateContext();
        var widget = TestWidget.Create("No Reliable Handler");
        await context.AddAsync(widget);
        var created = widget.DomainEvents.Single();

        await _fixture.CreateUnitOfWork(context, Substitute.For<IPublisher>()).SaveChangesAsync();

        await using var verify = _fixture.CreateContext();
        Assert.That(await verify.Set<OutboxMessage>().AnyAsync(m => m.Id == created.EventId), Is.False);
    }

    [Test]
    public async Task SaveChangesAsync_GivenSaveFailsOnUniqueViolation_WhenRolledBack_ThenOutboxHasNoOrphanEvent()
    {
        var code = Guid.NewGuid().ToString("N")[..20];
        await using (var seed = _fixture.CreateContext())
        {
            await seed.AddAsync(TestGadget.Create(code));
            await seed.SaveChangesAsync();
        }

        await using var context = _fixture.CreateContext();
        var widget = TestWidget.Create("Rolled Back");
        widget.IssueSecret("raw-token");
        var issued = widget.DomainEvents.OfType<TestSecretIssuedDomainEvent>().Single();
        await context.AddAsync(widget);
        await context.AddAsync(TestGadget.Create(code));

        Assert.ThrowsAsync<UniqueConstraintViolationException>(() =>
            _fixture.CreateUnitOfWork(context, Substitute.For<IPublisher>()).SaveChangesAsync());

        await using var verify = _fixture.CreateContext();
        Assert.That(await verify.Set<OutboxMessage>().AnyAsync(m => m.Id == issued.EventId), Is.False);
    }
}

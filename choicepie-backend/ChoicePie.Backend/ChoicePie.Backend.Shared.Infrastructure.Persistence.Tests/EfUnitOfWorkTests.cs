using ChoicePie.Backend.Shared.Application.Events;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests.TestSupport;
using MediatR;
using NSubstitute;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests;

public sealed class EfUnitOfWorkTests
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
    public async Task SaveChangesAsync_GivenEntityWithDomainEvent_WhenCalled_ThenPublishesDomainEventNotificationAfterCommit()
    {
        await using var context = _fixture.CreateContext();
        var publisher = Substitute.For<IPublisher>();
        var unitOfWork = _fixture.CreateUnitOfWork(context, publisher);
        var widget = TestWidget.Create("Widget With Event");
        await context.AddAsync(widget);

        await unitOfWork.SaveChangesAsync();

        await publisher.Received(1).Publish(
            Arg.Is<DomainEventNotification<TestWidgetCreatedDomainEvent>>(n => n.DomainEvent.WidgetId == widget.Id),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task SaveChangesAsync_GivenEntityWithDomainEvent_WhenCalled_ThenClearsDomainEventsAfterDispatch()
    {
        await using var context = _fixture.CreateContext();
        var publisher = Substitute.For<IPublisher>();
        var unitOfWork = _fixture.CreateUnitOfWork(context, publisher);
        var widget = TestWidget.Create("Widget Clears Events");
        await context.AddAsync(widget);

        await unitOfWork.SaveChangesAsync();

        Assert.That(widget.DomainEvents, Is.Empty);
    }

    [Test]
    public async Task SaveChangesAsync_GivenTrackedAggregateMutatedThenReloaded_WhenCalledThroughUnitOfWork_ThenPersistsTheChange()
    {
        await using var writeContext = _fixture.CreateContext();
        var widget = TestWidget.Create("Base Widget");
        await writeContext.AddAsync(widget);
        await writeContext.SaveChangesAsync();

        await using var mutateContext = _fixture.CreateContext();
        var repository = new TestWidgetRepository(mutateContext);
        var loaded = await repository.GetByIdAsync(widget.Id);
        loaded!.Rename("Mutated Widget");
        var unitOfWork = _fixture.CreateUnitOfWork(mutateContext, Substitute.For<IPublisher>());
        await unitOfWork.SaveChangesAsync();

        await using var verifyContext = _fixture.CreateContext();
        var verified = await new TestWidgetRepository(verifyContext).GetByIdAsync(widget.Id);
        Assert.That(verified!.Name, Is.EqualTo("Mutated Widget"));
    }
}

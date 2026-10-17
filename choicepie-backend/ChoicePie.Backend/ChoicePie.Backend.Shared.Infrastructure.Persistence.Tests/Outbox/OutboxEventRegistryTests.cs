using ChoicePie.Backend.Shared.Infrastructure.Persistence.Outbox;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests.TestSupport;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests.Outbox;

[TestFixture]
public class OutboxEventRegistryTests
{
    private readonly OutboxEventRegistry _registry = OutboxEventRegistry.FromAssemblies(typeof(TestWidget).Assembly);

    [Test]
    public void RequiresOutbox_GivenEventWithReliableHandler_WhenChecked_ThenReturnsTrue() =>
        Assert.That(_registry.RequiresOutbox(typeof(TestSecretIssuedDomainEvent)), Is.True);

    [Test]
    public void RequiresOutbox_GivenEventWithOnlyInProcessHandlers_WhenChecked_ThenReturnsFalse() =>
        Assert.That(_registry.RequiresOutbox(typeof(TestWidgetCreatedDomainEvent)), Is.False);
}

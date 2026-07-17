using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Shared.Kernel.Tests.Primitives;

public sealed record TestDomainEvent(string Message) : BaseDomainEvent;

public sealed class TestAggregate : AggregateRoot<Guid>
{
    public static TestAggregate Create()
    {
        var aggregate = new TestAggregate { Id = Guid.NewGuid() };
        aggregate.RaiseCreated();
        return aggregate;
    }

    private void RaiseCreated() => AddDomainEvent(new TestDomainEvent("created"));
}

[TestFixture]
public class AggregateRootTests
{
    [Test]
    public void AddDomainEvent_GivenNewAggregate_WhenCreated_ThenDomainEventsContainsTheRaisedEvent()
    {
        var aggregate = TestAggregate.Create();

        // BaseDomainEvent 帶了一個建構當下的時間戳記，即使 Message 相同，兩個個別建構的事件實例
        // 也永遠不會被判定相等（record 的結構相等會比較這個內部時間戳記），所以這裡改成比對
        // Message 本身，而不是整個事件物件的相等性。
        Assert.That(aggregate.DomainEvents, Has.Count.EqualTo(1));
        Assert.That(aggregate.DomainEvents.Single(), Is.InstanceOf<TestDomainEvent>());
        Assert.That(((TestDomainEvent)aggregate.DomainEvents.Single()).Message, Is.EqualTo("created"));
    }

    [Test]
    public void ClearDomainEvents_GivenAggregateWithPendingEvents_WhenCalled_ThenDomainEventsBecomesEmpty()
    {
        var aggregate = TestAggregate.Create();

        aggregate.ClearDomainEvents();

        Assert.That(aggregate.DomainEvents, Is.Empty);
    }
}

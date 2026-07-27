using ChoicePie.Backend.Shared.Application.Events;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Shared.Application.Tests.Events;

public sealed record TestDomainEvent(string Message) : BaseDomainEvent;

[TestFixture]
public class DomainEventNotificationTests
{
    [Test]
    public void DomainEvent_GivenConstructedWithAnEvent_WhenRead_ThenReturnsTheSameEventInstance()
    {
        var domainEvent = new TestDomainEvent("hello");

        var notification = new DomainEventNotification<TestDomainEvent>(domainEvent);

        Assert.That(notification.DomainEvent, Is.SameAs(domainEvent));
    }
}

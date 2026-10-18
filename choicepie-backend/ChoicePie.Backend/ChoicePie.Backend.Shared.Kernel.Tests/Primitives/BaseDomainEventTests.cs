using System.Text.Json;

namespace ChoicePie.Backend.Shared.Kernel.Tests.Primitives;

[TestFixture]
public class BaseDomainEventTests
{
    [Test]
    public void EventId_GivenTwoEventsWithSamePayload_WhenConstructed_ThenEachHasItsOwnNonEmptyId()
    {
        var first = new TestDomainEvent("same");
        var second = new TestDomainEvent("same");

        Assert.That(first.EventId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(first.EventId, Is.Not.EqualTo(second.EventId));
    }

    // Outbox 會把事件序列化存進資料庫、之後再還原；信封欄位必須原樣回來，消費端才能拿 EventId 去重。
    [Test]
    public void Envelope_GivenSerializedEvent_WhenDeserialized_ThenEventIdAndOccurredOnArePreserved()
    {
        var original = new TestDomainEvent("payload");

        var restored = JsonSerializer.Deserialize<TestDomainEvent>(JsonSerializer.Serialize(original))!;

        Assert.That(restored.EventId, Is.EqualTo(original.EventId));
        Assert.That(restored.OccurredOn, Is.EqualTo(original.OccurredOn));
        Assert.That(restored, Is.EqualTo(original));
    }
}

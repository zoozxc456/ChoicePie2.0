using ChoicePie.Backend.Shared.Infrastructure.Persistence.Outbox;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests.TestSupport;
using Microsoft.AspNetCore.DataProtection;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests.Outbox;

[TestFixture]
public class OutboxSerializerTests
{
    private OutboxSerializer _serializer = null!;

    [SetUp]
    public void SetUp() => _serializer = new OutboxSerializer(new EphemeralDataProtectionProvider());

    [Test]
    public void Deserialize_GivenSerializedEvent_WhenRestored_ThenEqualsTheOriginalIncludingEnvelope()
    {
        var original = new TestWidgetCreatedDomainEvent(Guid.NewGuid(), "Round Trip");

        var (type, payload) = _serializer.Serialize(original);
        var restored = _serializer.Deserialize(type, payload);

        Assert.That(restored, Is.EqualTo(original));
    }

    // 寄信事件帶著 raw token；資料庫只該存得到雜湊，Outbox 的 payload 不能讓明文 token 落地。
    [Test]
    public void Serialize_GivenEventCarryingSecret_WhenSerialized_ThenPayloadDoesNotContainTheSecret()
    {
        const string rawToken = "raw-token-that-must-not-be-stored";

        var (_, payload) = _serializer.Serialize(new TestSecretIssuedDomainEvent(Guid.NewGuid(), rawToken));

        Assert.That(payload, Does.Not.Contain(rawToken));
    }

    [Test]
    public void Deserialize_GivenTypeThatNoLongerExists_WhenRestored_ThenThrowsInvalidOperationException()
    {
        var (_, payload) = _serializer.Serialize(new TestWidgetCreatedDomainEvent(Guid.NewGuid(), "Orphan"));

        Assert.Throws<InvalidOperationException>(() =>
            _serializer.Deserialize("ChoicePie.Removed.SomeDomainEvent, ChoicePie.Removed", payload));
    }
}

using ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken.Events;
using ChoicePie.Backend.Domain.Aggregates.Member.Events;
using ChoicePie.Backend.Domain.Aggregates.PasswordResetToken.Events;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Events;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Outbox;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;
using Microsoft.AspNetCore.DataProtection;

namespace ChoicePie.Backend.Infrastructure.Tests.Persistence.Outbox;

/// <summary>
/// 有可靠 handler 的事件會被序列化進 outbox，之後再還原派送；還原不回來的事件會一路重試到 dead letter。
/// 新增可靠 handler 時這裡會先失敗，提醒補一個樣本、確認它真的能來回
/// （例如屬性型別是介面、或沒有 System.Text.Json 能用的建構子，就還原不回來）。
/// </summary>
[TestFixture]
public class ReliableEventContractTests
{
    private static readonly BaseDomainEvent[] Samples =
    [
        new PasswordResetRequestedDomainEvent(Guid.NewGuid(), "member@example.com", "raw-token"),
        new EmailVerificationRequestedDomainEvent(Guid.NewGuid(), "member@example.com", "raw-token"),
        new MemberAccountClosedDomainEvent(Guid.NewGuid()),
        new QuizAttemptCompletedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 87.5m, true)
    ];

    private readonly OutboxEventRegistry _registry =
        OutboxEventRegistry.FromAssemblies(typeof(Application.AssemblyReference).Assembly);

    [Test]
    public void Registry_GivenApplicationAssembly_WhenScanned_ThenEveryReliableEventHasARoundTripSample() =>
        Assert.That(_registry.EventTypes, Is.EquivalentTo(Samples.Select(s => s.GetType())));

    [TestCaseSource(nameof(Samples))]
    public void Serializer_GivenReliableEvent_WhenRoundTripped_ThenRestoresAnEqualEvent(BaseDomainEvent sample)
    {
        var serializer = new OutboxSerializer(new EphemeralDataProtectionProvider());

        var (type, payload) = serializer.Serialize(sample);

        Assert.That(serializer.Deserialize(type, payload), Is.EqualTo(sample));
    }
}

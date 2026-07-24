using ChoicePie.Backend.Domain.Aggregates.Member.Events;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using MemberAggregate = ChoicePie.Backend.Domain.Aggregates.Member.Member;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.Member;

[TestFixture]
public class MemberTests
{
    [Test]
    public void Create_GivenValidInput_WhenCalled_ThenCreatesMemberWithExpectedFields()
    {
        var member = MemberAggregate.Create("Host Name");

        Assert.Multiple(() =>
        {
            Assert.That(member.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(member.Name, Is.EqualTo("Host Name"));
            Assert.That(member.Avatar, Is.Null);
        });
    }

    [Test]
    public void Create_GivenValidInput_WhenCalled_ThenRaisesMemberCreatedDomainEvent()
    {
        var member = MemberAggregate.Create("Host Name");

        var domainEvent = member.DomainEvents.OfType<MemberCreatedDomainEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(domainEvent.MemberId, Is.EqualTo(member.Id));
            Assert.That(domainEvent.Name, Is.EqualTo("Host Name"));
        });
    }

    [TestCase("a")]
    [TestCase("this-name-is-way-too-long-for-a-host")]
    [TestCase("  ")]
    public void Create_GivenNameOutOfRange_WhenCalled_ThenThrowsInvalidMemberNameException(string name)
    {
        Assert.Throws<InvalidMemberNameException>(() => MemberAggregate.Create(name));
    }

    [Test]
    public void CanGenerateQuizToday_GivenNeverGenerated_WhenChecked_ThenReturnsTrue()
    {
        var member = MemberAggregate.Create("Host Name");

        Assert.That(member.CanGenerateQuizToday(DateTime.UtcNow), Is.True);
    }

    [Test]
    public void CanGenerateQuizToday_GivenGeneratedEarlierSameDay_WhenChecked_ThenReturnsFalse()
    {
        var member = MemberAggregate.Create("Host Name");
        var now = new DateTime(2026, 7, 10, 9, 0, 0, DateTimeKind.Utc);
        member.RecordAiGeneration(now);

        Assert.That(member.CanGenerateQuizToday(now.AddHours(5)), Is.False);
    }

    [Test]
    public void CanGenerateQuizToday_GivenGeneratedOnAPreviousDay_WhenChecked_ThenReturnsTrue()
    {
        var member = MemberAggregate.Create("Host Name");
        var yesterday = new DateTime(2026, 7, 9, 9, 0, 0, DateTimeKind.Utc);
        member.RecordAiGeneration(yesterday);

        Assert.That(member.CanGenerateQuizToday(yesterday.AddDays(1)), Is.True);
    }

    [Test]
    public void Suspend_GivenValidReason_WhenCalled_ThenSetsSuspensionFields()
    {
        var member = MemberAggregate.Create("Host Name");
        var until = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);

        member.Suspend("spamming", until);

        Assert.Multiple(() =>
        {
            Assert.That(member.IsSuspended, Is.True);
            Assert.That(member.SuspendedReason, Is.EqualTo("spamming"));
            Assert.That(member.SuspendedUntil, Is.EqualTo(until));
        });
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Suspend_GivenEmptyOrWhitespaceReason_WhenCalled_ThenThrowsInvalidMemberSuspensionException(string reason)
    {
        var member = MemberAggregate.Create("Host Name");

        Assert.Throws<InvalidMemberSuspensionException>(() => member.Suspend(reason, null));
    }

    [Test]
    public void Unsuspend_GivenSuspendedMember_WhenCalled_ThenClearsSuspensionFields()
    {
        var member = MemberAggregate.Create("Host Name");
        member.Suspend("spamming", null);

        member.Unsuspend();

        Assert.Multiple(() =>
        {
            Assert.That(member.IsSuspended, Is.False);
            Assert.That(member.SuspendedReason, Is.Null);
            Assert.That(member.SuspendedUntil, Is.Null);
        });
    }

    [Test]
    public void IsCurrentlySuspended_GivenNotSuspended_WhenChecked_ThenReturnsFalse()
    {
        var member = MemberAggregate.Create("Host Name");

        Assert.That(member.IsCurrentlySuspended(DateTime.UtcNow), Is.False);
    }

    [Test]
    public void IsCurrentlySuspended_GivenPermanentSuspension_WhenChecked_ThenReturnsTrue()
    {
        var member = MemberAggregate.Create("Host Name");
        member.Suspend("spamming", null);

        Assert.That(member.IsCurrentlySuspended(DateTime.UtcNow.AddYears(10)), Is.True);
    }

    [Test]
    public void IsCurrentlySuspended_GivenSuspensionNotYetExpired_WhenChecked_ThenReturnsTrue()
    {
        var member = MemberAggregate.Create("Host Name");
        var now = new DateTime(2026, 7, 10, 9, 0, 0, DateTimeKind.Utc);
        member.Suspend("spamming", now.AddDays(7));

        Assert.That(member.IsCurrentlySuspended(now.AddDays(1)), Is.True);
    }

    [Test]
    public void IsCurrentlySuspended_GivenSuspensionExpired_WhenChecked_ThenReturnsFalse()
    {
        var member = MemberAggregate.Create("Host Name");
        var now = new DateTime(2026, 7, 10, 9, 0, 0, DateTimeKind.Utc);
        member.Suspend("spamming", now.AddDays(7));

        Assert.That(member.IsCurrentlySuspended(now.AddDays(8)), Is.False);
    }
}

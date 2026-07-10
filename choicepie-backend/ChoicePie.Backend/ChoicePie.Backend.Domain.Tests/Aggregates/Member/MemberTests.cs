using ChoicePie.Backend.Domain.Aggregates.Member.Events;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using MemberAggregate = ChoicePie.Backend.Domain.Aggregates.Member.Member;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.Member;

[TestFixture]
public class MemberTests
{
    [Test]
    public void Register_GivenValidInput_WhenCalled_ThenCreatesMemberWithExpectedFields()
    {
        var email = Email.Create("host@example.com");

        var member = MemberAggregate.Register(email, "Host Name", "hashed-password");

        Assert.Multiple(() =>
        {
            Assert.That(member.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(member.Email, Is.EqualTo(email));
            Assert.That(member.Name, Is.EqualTo("Host Name"));
            Assert.That(member.PasswordHash, Is.EqualTo("hashed-password"));
            Assert.That(member.IsVerified, Is.False);
        });
    }

    [Test]
    public void Register_GivenValidInput_WhenCalled_ThenRaisesMemberRegisteredDomainEvent()
    {
        var email = Email.Create("host@example.com");

        var member = MemberAggregate.Register(email, "Host Name", "hashed-password");

        var domainEvent = member.DomainEvents.OfType<MemberRegisteredDomainEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(domainEvent.MemberId, Is.EqualTo(member.Id));
            Assert.That(domainEvent.Email, Is.EqualTo("host@example.com"));
            Assert.That(domainEvent.Name, Is.EqualTo("Host Name"));
        });
    }

    [TestCase("a")]
    [TestCase("this-name-is-way-too-long-for-a-host")]
    [TestCase("  ")]
    public void Register_GivenNameOutOfRange_WhenCalled_ThenThrowsInvalidMemberNameException(string name)
    {
        var email = Email.Create("host@example.com");

        Assert.Throws<InvalidMemberNameException>(() => MemberAggregate.Register(email, name, "hashed-password"));
    }

    [Test]
    public void CanGenerateQuizToday_GivenNeverGenerated_WhenChecked_ThenReturnsTrue()
    {
        var member = MemberAggregate.Register(Email.Create("host@example.com"), "Host Name", "hashed-password");

        Assert.That(member.CanGenerateQuizToday(DateTime.UtcNow), Is.True);
    }

    [Test]
    public void CanGenerateQuizToday_GivenGeneratedEarlierSameDay_WhenChecked_ThenReturnsFalse()
    {
        var member = MemberAggregate.Register(Email.Create("host@example.com"), "Host Name", "hashed-password");
        var now = new DateTime(2026, 7, 10, 9, 0, 0, DateTimeKind.Utc);
        member.RecordAiGeneration(now);

        Assert.That(member.CanGenerateQuizToday(now.AddHours(5)), Is.False);
    }

    [Test]
    public void CanGenerateQuizToday_GivenGeneratedOnAPreviousDay_WhenChecked_ThenReturnsTrue()
    {
        var member = MemberAggregate.Register(Email.Create("host@example.com"), "Host Name", "hashed-password");
        var yesterday = new DateTime(2026, 7, 9, 9, 0, 0, DateTimeKind.Utc);
        member.RecordAiGeneration(yesterday);

        Assert.That(member.CanGenerateQuizToday(yesterday.AddDays(1)), Is.True);
    }
}

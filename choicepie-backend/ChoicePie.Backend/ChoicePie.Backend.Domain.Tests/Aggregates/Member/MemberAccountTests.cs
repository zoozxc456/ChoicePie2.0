using ChoicePie.Backend.Domain.Aggregates.Member.Events;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using MemberAggregate = ChoicePie.Backend.Domain.Aggregates.Member.Member;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.Member;

[TestFixture]
public class MemberAccountTests
{
    [Test]
    public void Rename_GivenValidName_WhenCalled_ThenUpdatesName()
    {
        var member = MemberAggregate.Create("Host Name");

        member.Rename("New Name");

        Assert.That(member.Name, Is.EqualTo("New Name"));
    }

    [Test]
    public void Rename_GivenSurroundingWhitespace_WhenCalled_ThenStoresTrimmedName()
    {
        var member = MemberAggregate.Create("Host Name");

        member.Rename("  New Name  ");

        Assert.That(member.Name, Is.EqualTo("New Name"));
    }

    [TestCase("")]
    [TestCase(" a ")]
    [TestCase("this name is definitely longer than twenty characters")]
    public void Rename_GivenInvalidName_WhenCalled_ThenThrowsInvalidMemberNameException(string name)
    {
        var member = MemberAggregate.Create("Host Name");

        Assert.Throws<InvalidMemberNameException>(() => member.Rename(name));
    }

    [Test]
    public void ChangeAvatar_GivenUrl_WhenCalled_ThenSetsAvatarAndReturnsPreviousOne()
    {
        var member = MemberAggregate.Create("Host Name");
        member.ChangeAvatar("https://cdn.example.com/avatars/a.png");

        var previous = member.ChangeAvatar("https://cdn.example.com/avatars/b.png");

        Assert.Multiple(() =>
        {
            Assert.That(previous, Is.EqualTo("https://cdn.example.com/avatars/a.png"));
            Assert.That(member.Avatar, Is.EqualTo("https://cdn.example.com/avatars/b.png"));
        });
    }

    [Test]
    public void ChangeAvatar_GivenBlankUrl_WhenCalled_ThenThrowsArgumentException()
    {
        var member = MemberAggregate.Create("Host Name");

        Assert.Throws<ArgumentException>(() => member.ChangeAvatar(" "));
    }

    [Test]
    public void CloseAccount_GivenActiveMember_WhenCalled_ThenSoftDeletesAndRaisesAccountClosedEvent()
    {
        var member = MemberAggregate.Create("Host Name");

        member.CloseAccount();

        var domainEvent = member.DomainEvents.OfType<MemberAccountClosedDomainEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(member.IsDeleted, Is.True);
            Assert.That(member.DeleterId, Is.EqualTo(member.Id));
            Assert.That(domainEvent.MemberId, Is.EqualTo(member.Id));
        });
    }
}

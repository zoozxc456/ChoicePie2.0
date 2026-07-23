using ChoicePie.Backend.Domain.Aggregates.CreatorFollow.Exceptions;
using CreatorFollowAggregate = ChoicePie.Backend.Domain.Aggregates.CreatorFollow.CreatorFollow;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.CreatorFollow;

[TestFixture]
public class CreatorFollowTests
{
    [Test]
    public void Create_GivenFollowerAndCreatorId_WhenCalled_ThenCreatesFollowWithExpectedFields()
    {
        var followerId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();

        var follow = CreatorFollowAggregate.Create(followerId, creatorId);

        Assert.Multiple(() =>
        {
            Assert.That(follow.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(follow.FollowerId, Is.EqualTo(followerId));
            Assert.That(follow.FollowedCreatorId, Is.EqualTo(creatorId));
        });
    }

    [Test]
    public void Create_GivenSameFollowerAndCreatorId_WhenCalled_ThenThrowsCannotFollowSelfException()
    {
        var userId = Guid.NewGuid();

        Assert.Throws<CannotFollowSelfException>(() => CreatorFollowAggregate.Create(userId, userId));
    }
}

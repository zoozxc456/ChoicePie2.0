using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.GameRoom.ValueObjects;

[TestFixture]
public class RankEntrySnapshotTests
{
    [Test]
    public void Equals_GivenSamePlayerIdNicknameScoreAndRank_WhenCompared_ThenAreEqual()
    {
        var playerId = Guid.NewGuid();

        var a = new RankEntrySnapshot(playerId, "Alice", 100, 1);
        var b = new RankEntrySnapshot(playerId, "Alice", 100, 1);

        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void Equals_GivenDifferentRank_WhenCompared_ThenAreNotEqual()
    {
        var playerId = Guid.NewGuid();

        var a = new RankEntrySnapshot(playerId, "Alice", 100, 1);
        var b = new RankEntrySnapshot(playerId, "Alice", 100, 2);

        Assert.That(a, Is.Not.EqualTo(b));
    }
}

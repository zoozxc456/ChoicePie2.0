using ChoicePie.Backend.Domain.Aggregates.GameSession.ValueObjects;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.GameSession.ValueObjects;

[TestFixture]
public class GameSessionPlayerResultTests
{
    [Test]
    public void Equals_GivenSameFieldsAndSameAnswersListInstance_WhenCompared_ThenAreEqual()
    {
        var playerId = Guid.NewGuid();
        var answers = new List<GameSessionAnswerLogEntry> { new(0, 1, true, 100) };

        var a = new GameSessionPlayerResult(playerId, "Alice", null, 100, 1, answers);
        var b = new GameSessionPlayerResult(playerId, "Alice", null, 100, 1, answers);

        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void Equals_GivenDifferentMemberId_WhenCompared_ThenAreNotEqual()
    {
        var playerId = Guid.NewGuid();
        var answers = new List<GameSessionAnswerLogEntry>();

        var a = new GameSessionPlayerResult(playerId, "Alice", Guid.NewGuid(), 100, 1, answers);
        var b = new GameSessionPlayerResult(playerId, "Alice", null, 100, 1, answers);

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public void MemberId_GivenAnonymousPlayer_WhenConstructed_ThenIsNull()
    {
        var result = new GameSessionPlayerResult(Guid.NewGuid(), "Bob", null, 0, 2, []);

        Assert.That(result.MemberId, Is.Null);
    }
}

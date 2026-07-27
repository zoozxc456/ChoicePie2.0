using ChoicePie.Backend.Domain.Aggregates.GameSession.ValueObjects;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.GameSession.ValueObjects;

[TestFixture]
public class GameSessionAnswerLogEntryTests
{
    [Test]
    public void Equals_GivenSameQuestionIndexSelectedOptionIsCorrectAndScoreAwarded_WhenCompared_ThenAreEqual()
    {
        var a = new GameSessionAnswerLogEntry(0, 1, true, 100);
        var b = new GameSessionAnswerLogEntry(0, 1, true, 100);

        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void Equals_GivenDifferentScoreAwarded_WhenCompared_ThenAreNotEqual()
    {
        var a = new GameSessionAnswerLogEntry(0, 1, true, 100);
        var b = new GameSessionAnswerLogEntry(0, 1, true, 50);

        Assert.That(a, Is.Not.EqualTo(b));
    }
}

using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.GameRoom.ValueObjects;

[TestFixture]
public class AnswerOutcomeTests
{
    [Test]
    public void Equals_GivenSameScoreIsCorrectAndCorrectAnswerIndex_WhenCompared_ThenAreEqual()
    {
        var a = new AnswerOutcome(100, true, 1);
        var b = new AnswerOutcome(100, true, 1);

        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void Equals_GivenDifferentIsCorrect_WhenCompared_ThenAreNotEqual()
    {
        var a = new AnswerOutcome(0, false, 1);
        var b = new AnswerOutcome(0, true, 1);

        Assert.That(a, Is.Not.EqualTo(b));
    }
}

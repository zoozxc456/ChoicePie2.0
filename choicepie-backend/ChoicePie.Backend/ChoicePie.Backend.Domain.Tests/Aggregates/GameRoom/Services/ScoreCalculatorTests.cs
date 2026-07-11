using ChoicePie.Backend.Domain.Aggregates.GameRoom.Services;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.GameRoom.Services;

[TestFixture]
public class ScoreCalculatorTests
{
    [Test]
    public void Calculate_GivenIncorrectAnswer_WhenCalled_ThenReturnsZero()
    {
        var score = ScoreCalculator.Calculate(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(20), isCorrect: false);

        Assert.That(score, Is.EqualTo(0));
    }

    [TestCase(0, 1000)]
    [TestCase(3, 1000)]
    [TestCase(3.5, 800)]
    [TestCase(6, 800)]
    [TestCase(6.5, 600)]
    [TestCase(10, 600)]
    [TestCase(10.5, 400)]
    [TestCase(19.99, 400)]
    public void Calculate_GivenCorrectAnswerWithinTimeLimit_WhenCalled_ThenReturnsPointsForElapsedBand(
        double elapsedSeconds, int expectedScore)
    {
        var score = ScoreCalculator.Calculate(
            TimeSpan.FromSeconds(elapsedSeconds), TimeSpan.FromSeconds(20), isCorrect: true);

        Assert.That(score, Is.EqualTo(expectedScore));
    }

    [Test]
    public void Calculate_GivenAnswerAtOrAfterTimeLimit_WhenCalled_ThenReturnsZero()
    {
        var score = ScoreCalculator.Calculate(TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20), isCorrect: true);

        Assert.That(score, Is.EqualTo(0));
    }

    [Test]
    public void Calculate_GivenNegativeElapsed_WhenCalled_ThenReturnsZero()
    {
        var score = ScoreCalculator.Calculate(TimeSpan.FromSeconds(-1), TimeSpan.FromSeconds(20), isCorrect: true);

        Assert.That(score, Is.EqualTo(0));
    }
}

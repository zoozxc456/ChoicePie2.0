using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.ValueObjects;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.QuizAttempt.ValueObjects;

[TestFixture]
public class AttemptScoreTests
{
    [Test]
    public void FromCorrectCount_GivenAllAnswersCorrect_WhenCalled_ThenValueIsHundredAndPassedIsTrue()
    {
        var score = AttemptScore.FromCorrectCount(correctCount: 2, totalCount: 2);

        Assert.Multiple(() =>
        {
            Assert.That(score.Value, Is.EqualTo(100m));
            Assert.That(score.Passed, Is.True);
        });
    }

    [Test]
    public void FromCorrectCount_GivenNoAnswersCorrect_WhenCalled_ThenValueIsZeroAndPassedIsFalse()
    {
        var score = AttemptScore.FromCorrectCount(correctCount: 0, totalCount: 2);

        Assert.Multiple(() =>
        {
            Assert.That(score.Value, Is.EqualTo(0m));
            Assert.That(score.Passed, Is.False);
        });
    }

    [Test]
    public void FromCorrectCount_GivenValueExactlyAtPassThreshold_WhenCalled_ThenPassedIsTrue()
    {
        var score = AttemptScore.FromCorrectCount(correctCount: 3, totalCount: 5);

        Assert.Multiple(() =>
        {
            Assert.That(score.Value, Is.EqualTo(60m));
            Assert.That(score.Passed, Is.True);
        });
    }
}

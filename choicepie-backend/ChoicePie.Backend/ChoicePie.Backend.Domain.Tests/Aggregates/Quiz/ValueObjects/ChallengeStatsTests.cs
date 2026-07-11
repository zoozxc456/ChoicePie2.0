using ChoicePie.Backend.Domain.Aggregates.Quiz.ValueObjects;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.Quiz.ValueObjects;

[TestFixture]
public class ChallengeStatsTests
{
    [Test]
    public void None_WhenAccessed_ThenHasZeroCountAndZeroPassRate()
    {
        var stats = ChallengeStats.None;

        Assert.Multiple(() =>
        {
            Assert.That(stats.Count, Is.EqualTo(0));
            Assert.That(stats.PassRate, Is.EqualTo(0m));
        });
    }

    [Test]
    public void RecordOutcome_GivenFirstPassedOutcomeFromNone_WhenCalled_ThenReturnsCountOneAndPassRateHundred()
    {
        var updated = ChallengeStats.None.RecordOutcome(passed: true);

        Assert.Multiple(() =>
        {
            Assert.That(updated.Count, Is.EqualTo(1));
            Assert.That(updated.PassRate, Is.EqualTo(100m));
        });
    }

    [Test]
    public void RecordOutcome_GivenOnePassedThenOneFailed_WhenCalled_ThenPassRateIsFifty()
    {
        var updated = ChallengeStats.None
            .RecordOutcome(passed: true)
            .RecordOutcome(passed: false);

        Assert.Multiple(() =>
        {
            Assert.That(updated.Count, Is.EqualTo(2));
            Assert.That(updated.PassRate, Is.EqualTo(50m));
        });
    }
}

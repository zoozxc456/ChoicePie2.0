using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Enums;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.QuizAttempt.Enums;

[TestFixture]
public class AttemptStatusTests
{
    [Test]
    public void FromName_GivenKnownNameDifferentCase_WhenCalled_ThenReturnsMatchingInstance()
    {
        var status = AttemptStatus.FromName("COMPLETED");

        Assert.That(status, Is.EqualTo(AttemptStatus.Completed));
    }

    [Test]
    public void Enumerations_WhenRead_ThenContainsExactlyTwoStatuses()
    {
        Assert.That(AttemptStatus.Enumerations.Values, Is.EquivalentTo(new[]
        {
            AttemptStatus.InProgress, AttemptStatus.Completed
        }));
    }
}

using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.Quiz.Enums;

[TestFixture]
public class QuizStatusTests
{
    [Test]
    public void FromName_GivenKnownNameDifferentCase_WhenCalled_ThenReturnsMatchingInstance()
    {
        var status = QuizStatus.FromName("PUBLISHED");

        Assert.That(status, Is.EqualTo(QuizStatus.Published));
    }

    [Test]
    public void FromName_GivenUnknownName_WhenCalled_ThenReturnsNull()
    {
        Assert.That(QuizStatus.FromName("deleted"), Is.Null);
    }

    [Test]
    public void Enumerations_WhenRead_ThenContainsExactlyThreeStatuses()
    {
        Assert.That(QuizStatus.Enumerations.Values, Is.EquivalentTo(new[]
        {
            QuizStatus.Draft, QuizStatus.Published, QuizStatus.Archived
        }));
    }
}

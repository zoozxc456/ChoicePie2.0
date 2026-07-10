using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.Quiz.Enums;

[TestFixture]
public class DifficultyTests
{
    [Test]
    public void FromName_GivenKnownNameDifferentCase_WhenCalled_ThenReturnsMatchingInstance()
    {
        var difficulty = Difficulty.FromName("Beginner");

        Assert.That(difficulty, Is.EqualTo(Difficulty.Beginner));
    }

    [Test]
    public void FromName_GivenUnknownName_WhenCalled_ThenReturnsNull()
    {
        Assert.That(Difficulty.FromName("legendary"), Is.Null);
    }

    [Test]
    public void FromValue_GivenKnownId_WhenCalled_ThenReturnsMatchingInstance()
    {
        Assert.That(Difficulty.FromValue(2), Is.EqualTo(Difficulty.Intermediate));
    }

    [Test]
    public void Enumerations_WhenRead_ThenContainsExactlyThreeLevels()
    {
        Assert.That(Difficulty.Enumerations.Values, Is.EquivalentTo(new[]
        {
            Difficulty.Beginner, Difficulty.Intermediate, Difficulty.Expert
        }));
    }
}

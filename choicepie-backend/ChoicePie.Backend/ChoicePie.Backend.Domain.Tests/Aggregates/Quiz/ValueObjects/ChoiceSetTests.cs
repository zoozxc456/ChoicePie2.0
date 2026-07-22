using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz.ValueObjects;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.Quiz.ValueObjects;

[TestFixture]
public class ChoiceSetTests
{
    private static readonly string[] FourOptions = ["A", "B", "C", "D"];

    [Test]
    public void Create_GivenFourOptionsAndInRangeAnswerIndex_WhenCalled_ThenReturnsChoiceSetWithExpectedFields()
    {
        var choiceSet = ChoiceSet.Create(FourOptions, 1);

        Assert.Multiple(() =>
        {
            Assert.That(choiceSet.Options, Is.EqualTo(FourOptions));
            Assert.That(choiceSet.AnswerIndex, Is.EqualTo(1));
        });
    }

    [TestCaseSource(nameof(InvalidOptionSets))]
    public void Create_GivenOptionCountNotFour_WhenCalled_ThenThrowsInvalidQuestionException(string[] options)
    {
        Assert.Throws<InvalidQuestionException>(() => ChoiceSet.Create(options, 0));
    }

    private static IEnumerable<string[]> InvalidOptionSets()
    {
        yield return ["A", "B", "C"];
        yield return ["A", "B", "C", "D", "E"];
        yield return [];
    }

    [Test]
    public void Create_GivenOptionContainingBlank_WhenCalled_ThenThrowsInvalidQuestionException()
    {
        Assert.Throws<InvalidQuestionException>(() => ChoiceSet.Create(["A", "  ", "C", "D"], 0));
    }

    [TestCase(-1)]
    [TestCase(4)]
    public void Create_GivenAnswerIndexOutOfRange_WhenCalled_ThenThrowsInvalidQuestionException(int answerIndex)
    {
        Assert.Throws<InvalidQuestionException>(() => ChoiceSet.Create(FourOptions, answerIndex));
    }
}

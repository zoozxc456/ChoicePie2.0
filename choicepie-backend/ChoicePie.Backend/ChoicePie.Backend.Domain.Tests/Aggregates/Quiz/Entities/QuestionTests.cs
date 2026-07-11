using ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.Quiz.Entities;

[TestFixture]
public class QuestionTests
{
    private static readonly string[] FourOptions = ["A", "B", "C", "D"];

    [Test]
    public void Create_GivenValidInput_WhenCalled_ThenReturnsQuestionWithExpectedFields()
    {
        var question = Question.Create("2+2=?", FourOptions, 1, "因為 2+2=4");

        Assert.Multiple(() =>
        {
            Assert.That(question.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(question.Text, Is.EqualTo("2+2=?"));
            Assert.That(question.Options, Is.EqualTo(FourOptions));
            Assert.That(question.AnswerIndex, Is.EqualTo(1));
            Assert.That(question.Explanation, Is.EqualTo("因為 2+2=4"));
        });
    }

    [TestCaseSource(nameof(InvalidOptionSets))]
    public void Create_GivenOptionCountNotFour_WhenCalled_ThenThrowsInvalidQuestionException(string[] options)
    {
        Assert.Throws<InvalidQuestionException>(() => Question.Create("Q?", options, 0, "because"));
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
        Assert.Throws<InvalidQuestionException>(() => Question.Create("Q?", ["A", "  ", "C", "D"], 0, "because"));
    }

    [TestCase(-1)]
    [TestCase(4)]
    public void Create_GivenAnswerIndexOutOfRange_WhenCalled_ThenThrowsInvalidQuestionException(int answerIndex)
    {
        Assert.Throws<InvalidQuestionException>(() => Question.Create("Q?", FourOptions, answerIndex, "because"));
    }

    [Test]
    public void Create_GivenBlankText_WhenCalled_ThenThrowsInvalidQuestionException()
    {
        Assert.Throws<InvalidQuestionException>(() => Question.Create("   ", FourOptions, 0, "because"));
    }

    [Test]
    public void Update_GivenValidInput_WhenCalled_ThenReplacesFields()
    {
        var question = Question.Create("2+2=?", FourOptions, 1, "因為 2+2=4");

        question.Update("3+3=?", ["4", "5", "6", "7"], 2, "因為 3+3=6");

        Assert.Multiple(() =>
        {
            Assert.That(question.Text, Is.EqualTo("3+3=?"));
            Assert.That(question.Options, Is.EqualTo(new[] { "4", "5", "6", "7" }));
            Assert.That(question.AnswerIndex, Is.EqualTo(2));
            Assert.That(question.Explanation, Is.EqualTo("因為 3+3=6"));
        });
    }

    [Test]
    public void Update_GivenBlankText_WhenCalled_ThenThrowsInvalidQuestionException()
    {
        var question = Question.Create("2+2=?", FourOptions, 1, "因為 2+2=4");

        Assert.Throws<InvalidQuestionException>(() => question.Update("   ", FourOptions, 0, "because"));
    }
}

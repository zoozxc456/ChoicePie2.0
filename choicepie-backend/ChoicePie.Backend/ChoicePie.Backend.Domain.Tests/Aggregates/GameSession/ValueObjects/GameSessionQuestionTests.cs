using ChoicePie.Backend.Domain.Aggregates.GameSession.ValueObjects;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.GameSession.ValueObjects;

[TestFixture]
public class GameSessionQuestionTests
{
    [Test]
    public void Equals_GivenSameQuestionIdAndSameOptionsListInstance_WhenCompared_ThenAreEqual()
    {
        var questionId = Guid.NewGuid();
        var options = new List<string> { "1", "2", "3", "4" };

        var a = new GameSessionQuestion(questionId, "1+1=?", options, 1, "e");
        var b = new GameSessionQuestion(questionId, "1+1=?", options, 1, "e");

        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void Equals_GivenDifferentAnswerIndex_WhenCompared_ThenAreNotEqual()
    {
        var questionId = Guid.NewGuid();
        var options = new List<string> { "1", "2", "3", "4" };

        var a = new GameSessionQuestion(questionId, "1+1=?", options, 1, "e");
        var b = new GameSessionQuestion(questionId, "1+1=?", options, 2, "e");

        Assert.That(a, Is.Not.EqualTo(b));
    }
}

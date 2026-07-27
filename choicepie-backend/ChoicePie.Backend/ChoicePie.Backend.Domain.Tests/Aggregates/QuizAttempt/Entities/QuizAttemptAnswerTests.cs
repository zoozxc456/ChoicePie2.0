using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Entities;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.QuizAttempt.Entities;

[TestFixture]
public class QuizAttemptAnswerTests
{
    [Test]
    public void Create_GivenQuestionIdSelectedIndexAndAnsweredAt_WhenCalled_ThenSetsAllFields()
    {
        var questionId = Guid.NewGuid();
        var answeredAt = new DateTime(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc);

        var answer = QuizAttemptAnswer.Create(questionId, 2, answeredAt);

        Assert.Multiple(() =>
        {
            Assert.That(answer.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(answer.QuestionId, Is.EqualTo(questionId));
            Assert.That(answer.SelectedOptionIndex, Is.EqualTo(2));
            Assert.That(answer.AnsweredAt, Is.EqualTo(answeredAt));
        });
    }

    [Test]
    public void Create_GivenCalledTwice_WhenCalled_ThenEachAnswerGetsAUniqueId()
    {
        var first = QuizAttemptAnswer.Create(Guid.NewGuid(), 0, DateTime.UtcNow);
        var second = QuizAttemptAnswer.Create(Guid.NewGuid(), 0, DateTime.UtcNow);

        Assert.That(first.Id, Is.Not.EqualTo(second.Id));
    }
}

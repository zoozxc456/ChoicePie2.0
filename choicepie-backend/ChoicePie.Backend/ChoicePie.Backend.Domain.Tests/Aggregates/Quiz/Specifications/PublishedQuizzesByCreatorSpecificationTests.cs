using ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Specifications;
using QuizAggregate = ChoicePie.Backend.Domain.Aggregates.Quiz.Quiz;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.Quiz.Specifications;

[TestFixture]
public class PublishedQuizzesByCreatorSpecificationTests
{
    private static QuizAggregate CreateQuiz(Guid creatorId, bool publish)
    {
        var quiz = QuizAggregate.Create(creatorId, "Title", null, null, "x", "primary",Difficulty.Beginner, ["tag"]);
        if (publish)
        {
            quiz.AddQuestion(Question.Create("2+2=?", ["1", "2", "3", "4"], 3, "basic math"));
            quiz.Publish();
        }

        return quiz;
    }

    [Test]
    public void ToExpression_GivenPublishedQuizOfCreator_WhenCompiledAndInvoked_ThenReturnsTrue()
    {
        var creatorId = Guid.NewGuid();
        var specification = new PublishedQuizzesByCreatorSpecification(creatorId);

        Assert.That(specification.ToExpression().Compile()(CreateQuiz(creatorId, publish: true)), Is.True);
    }

    [Test]
    public void ToExpression_GivenDraftQuizOfCreator_WhenCompiledAndInvoked_ThenReturnsFalse()
    {
        var creatorId = Guid.NewGuid();
        var specification = new PublishedQuizzesByCreatorSpecification(creatorId);

        Assert.That(specification.ToExpression().Compile()(CreateQuiz(creatorId, publish: false)), Is.False);
    }

    [Test]
    public void ToExpression_GivenPublishedQuizOfOtherCreator_WhenCompiledAndInvoked_ThenReturnsFalse()
    {
        var specification = new PublishedQuizzesByCreatorSpecification(Guid.NewGuid());

        Assert.That(specification.ToExpression().Compile()(CreateQuiz(Guid.NewGuid(), publish: true)), Is.False);
    }
}

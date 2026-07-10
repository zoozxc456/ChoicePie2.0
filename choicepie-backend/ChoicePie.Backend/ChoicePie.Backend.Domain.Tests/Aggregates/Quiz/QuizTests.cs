using ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Events;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using Difficulty = ChoicePie.Backend.Domain.Aggregates.Quiz.Enums.Difficulty;
using QuizAggregate = ChoicePie.Backend.Domain.Aggregates.Quiz.Quiz;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.Quiz;

[TestFixture]
public class QuizTests
{
    private static readonly Guid CreatorId = Guid.NewGuid();

    private static QuizAggregate CreateQuiz(IEnumerable<string>? tags = null) => QuizAggregate.Create(
        creatorId: CreatorId,
        title: "Kubernetes 101",
        description: "A quiz about k8s",
        coverEmoji: "⚓",
        coverGradient: "background: linear-gradient(135deg,#0f3460,#533483);",
        difficulty: Difficulty.Beginner,
        isPublic: true,
        tags: tags ?? ["Kubernetes"]);

    [Test]
    public void Create_GivenValidInput_WhenCalled_ThenCreatesQuizWithExpectedFields()
    {
        var quiz = CreateQuiz();

        Assert.Multiple(() =>
        {
            Assert.That(quiz.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(quiz.OwnerId, Is.EqualTo(CreatorId));
            Assert.That(quiz.Title, Is.EqualTo("Kubernetes 101"));
            Assert.That(quiz.Difficulty, Is.EqualTo(Difficulty.Beginner));
            Assert.That(quiz.IsPublic, Is.True);
            Assert.That(quiz.QuestionCount, Is.EqualTo(0));
            Assert.That(quiz.ChallengeCount, Is.EqualTo(0));
            Assert.That(quiz.PassRate, Is.EqualTo(0));
            Assert.That(quiz.Tags, Is.EqualTo(new[] { "Kubernetes" }));
        });
    }

    [Test]
    public void Create_GivenDuplicateTags_WhenCalled_ThenDeduplicatesTags()
    {
        var quiz = CreateQuiz(tags: ["Go", "go", "Go"]);

        Assert.That(quiz.Tags, Has.Count.EqualTo(1));
    }

    [Test]
    public void Create_GivenValidInput_WhenCalled_ThenRaisesQuizCreatedDomainEvent()
    {
        var quiz = CreateQuiz();

        var domainEvent = quiz.DomainEvents.OfType<QuizCreatedDomainEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(domainEvent.QuizId, Is.EqualTo(quiz.Id));
            Assert.That(domainEvent.CreatorId, Is.EqualTo(CreatorId));
            Assert.That(domainEvent.Title, Is.EqualTo("Kubernetes 101"));
        });
    }

    [Test]
    public void Create_GivenBlankTitle_WhenCalled_ThenThrowsInvalidQuizException()
    {
        Assert.Throws<InvalidQuizException>(() => QuizAggregate.Create(
            CreatorId, "   ", null, "⚓", "gradient", Difficulty.Beginner, true, []));
    }

    [Test]
    public void AddQuestion_WhenCalled_ThenAddsQuestionAndIncrementsQuestionCount()
    {
        var quiz = CreateQuiz();
        var question = Question.Create("2+2=?", ["1", "2", "3", "4"], 3, "basic math");

        quiz.AddQuestion(question);

        Assert.Multiple(() =>
        {
            Assert.That(quiz.Questions, Has.Count.EqualTo(1));
            Assert.That(quiz.QuestionCount, Is.EqualTo(1));
            Assert.That(quiz.Questions[0], Is.EqualTo(question));
        });
    }

    [Test]
    public void RemoveQuestion_GivenExistingQuestionId_WhenCalled_ThenRemovesIt()
    {
        var quiz = CreateQuiz();
        var question = Question.Create("2+2=?", ["1", "2", "3", "4"], 3, "basic math");
        quiz.AddQuestion(question);

        quiz.RemoveQuestion(question.Id);

        Assert.That(quiz.QuestionCount, Is.EqualTo(0));
    }

    [Test]
    public void UpdateDetails_WhenCalled_ThenUpdatesTitleDescriptionVisibilityAndTags()
    {
        var quiz = CreateQuiz(tags: ["Old"]);

        quiz.UpdateDetails("New Title", "New Description", false, ["New", "New"]);

        Assert.Multiple(() =>
        {
            Assert.That(quiz.Title, Is.EqualTo("New Title"));
            Assert.That(quiz.Description, Is.EqualTo("New Description"));
            Assert.That(quiz.IsPublic, Is.False);
            Assert.That(quiz.Tags, Is.EqualTo(new[] { "New" }));
        });
    }

    [Test]
    public void UpdateDetails_GivenBlankTitle_WhenCalled_ThenThrowsInvalidQuizException()
    {
        var quiz = CreateQuiz();

        Assert.Throws<InvalidQuizException>(() => quiz.UpdateDetails("  ", null, true, []));
    }

    [Test]
    public void Publish_WhenCalled_ThenSetsIsPublicTrue()
    {
        var quiz = QuizAggregate.Create(CreatorId, "T", null, "⚓", "g", Difficulty.Beginner, false, []);

        quiz.Publish();

        Assert.That(quiz.IsPublic, Is.True);
    }

    [Test]
    public void Unpublish_WhenCalled_ThenSetsIsPublicFalse()
    {
        var quiz = CreateQuiz();

        quiz.Unpublish();

        Assert.That(quiz.IsPublic, Is.False);
    }

    [Test]
    public void EnsureModifiableBy_GivenCreator_WhenCalled_ThenDoesNotThrow()
    {
        var quiz = CreateQuiz();

        Assert.DoesNotThrow(() => quiz.EnsureModifiableBy(CreatorId));
    }

    [Test]
    public void EnsureModifiableBy_GivenNonCreator_WhenCalled_ThenThrowsQuizForbiddenException()
    {
        var quiz = CreateQuiz();

        Assert.Throws<QuizForbiddenException>(() => quiz.EnsureModifiableBy(Guid.NewGuid()));
    }
}

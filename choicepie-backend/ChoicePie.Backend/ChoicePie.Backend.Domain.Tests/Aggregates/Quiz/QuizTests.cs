using ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Events;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using Difficulty = ChoicePie.Backend.Domain.Aggregates.Quiz.Enums.Difficulty;
using QuizAggregate = ChoicePie.Backend.Domain.Aggregates.Quiz.Quiz;
using QuizStatus = ChoicePie.Backend.Domain.Aggregates.Quiz.Enums.QuizStatus;

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
        tags: tags ?? ["Kubernetes"]);

    private static Question CreateQuestion() => Question.Create("2+2=?", ["1", "2", "3", "4"], 3, "basic math");

    private static QuizAggregate CreatePublishedQuiz()
    {
        var quiz = CreateQuiz();
        quiz.AddQuestion(CreateQuestion());
        quiz.Publish();
        return quiz;
    }

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
            Assert.That(quiz.Status, Is.EqualTo(QuizStatus.Draft));
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
            CreatorId, "   ", null, "⚓", "gradient", Difficulty.Beginner, []));
    }

    [Test]
    public void AddQuestion_WhenCalled_ThenAddsQuestionAndIncrementsQuestionCount()
    {
        var quiz = CreateQuiz();
        var question = CreateQuestion();

        quiz.AddQuestion(question);

        Assert.Multiple(() =>
        {
            Assert.That(quiz.Questions, Has.Count.EqualTo(1));
            Assert.That(quiz.QuestionCount, Is.EqualTo(1));
            Assert.That(quiz.Questions[0], Is.EqualTo(question));
        });
    }

    [Test]
    public void AddQuestion_GivenPublishedQuiz_WhenCalled_ThenThrowsInvalidQuizException()
    {
        var quiz = CreatePublishedQuiz();

        Assert.Throws<InvalidQuizException>(() => quiz.AddQuestion(CreateQuestion()));
    }

    [Test]
    public void AddQuestion_GivenArchivedQuiz_WhenCalled_ThenThrowsInvalidQuizException()
    {
        var quiz = CreatePublishedQuiz();
        quiz.Archive();

        Assert.Throws<InvalidQuizException>(() => quiz.AddQuestion(CreateQuestion()));
    }

    [Test]
    public void RemoveQuestion_GivenExistingQuestionId_WhenCalled_ThenRemovesIt()
    {
        var quiz = CreateQuiz();
        var question = CreateQuestion();
        quiz.AddQuestion(question);

        quiz.RemoveQuestion(question.Id);

        Assert.That(quiz.QuestionCount, Is.EqualTo(0));
    }

    [Test]
    public void RemoveQuestion_GivenPublishedQuiz_WhenCalled_ThenThrowsInvalidQuizException()
    {
        var quiz = CreatePublishedQuiz();
        var questionId = quiz.Questions[0].Id;

        Assert.Throws<InvalidQuizException>(() => quiz.RemoveQuestion(questionId));
    }

    [Test]
    public void UpdateQuestion_GivenExistingQuestionId_WhenCalled_ThenReplacesQuestionFields()
    {
        var quiz = CreateQuiz();
        var question = CreateQuestion();
        quiz.AddQuestion(question);

        quiz.UpdateQuestion(question.Id, "1+1=?", ["1", "2", "3", "4"], 1, "basic math");

        Assert.Multiple(() =>
        {
            Assert.That(quiz.Questions[0].Text, Is.EqualTo("1+1=?"));
            Assert.That(quiz.Questions[0].AnswerIndex, Is.EqualTo(1));
        });
    }

    [Test]
    public void UpdateQuestion_GivenUnknownQuestionId_WhenCalled_ThenThrowsInvalidQuestionException()
    {
        var quiz = CreateQuiz();

        Assert.Throws<InvalidQuestionException>(() =>
            quiz.UpdateQuestion(Guid.NewGuid(), "Q?", ["1", "2", "3", "4"], 0, "why"));
    }

    [Test]
    public void UpdateQuestion_GivenPublishedQuiz_WhenCalled_ThenThrowsInvalidQuizException()
    {
        var quiz = CreatePublishedQuiz();
        var questionId = quiz.Questions[0].Id;

        Assert.Throws<InvalidQuizException>(() =>
            quiz.UpdateQuestion(questionId, "Q?", ["1", "2", "3", "4"], 0, "why"));
    }

    [Test]
    public void UpdateDetails_WhenCalled_ThenUpdatesTitleDescriptionAndTags()
    {
        var quiz = CreateQuiz(tags: ["Old"]);

        quiz.UpdateDetails("New Title", "New Description", ["New", "New"]);

        Assert.Multiple(() =>
        {
            Assert.That(quiz.Title, Is.EqualTo("New Title"));
            Assert.That(quiz.Description, Is.EqualTo("New Description"));
            Assert.That(quiz.Tags, Is.EqualTo(new[] { "New" }));
        });
    }

    [Test]
    public void UpdateDetails_GivenBlankTitle_WhenCalled_ThenThrowsInvalidQuizException()
    {
        var quiz = CreateQuiz();

        Assert.Throws<InvalidQuizException>(() => quiz.UpdateDetails("  ", null, []));
    }

    [Test]
    public void UpdateDetails_GivenArchivedQuiz_WhenCalled_ThenThrowsInvalidQuizException()
    {
        var quiz = CreatePublishedQuiz();
        quiz.Archive();

        Assert.Throws<InvalidQuizException>(() => quiz.UpdateDetails("New", null, []));
    }

    [Test]
    public void Publish_GivenDraftQuizWithQuestions_WhenCalled_ThenSetsStatusPublished()
    {
        var quiz = CreateQuiz();
        quiz.AddQuestion(CreateQuestion());

        quiz.Publish();

        Assert.That(quiz.Status, Is.EqualTo(QuizStatus.Published));
    }

    [Test]
    public void Publish_GivenQuizWithoutQuestions_WhenCalled_ThenThrowsInvalidQuizException()
    {
        var quiz = CreateQuiz();

        Assert.Throws<InvalidQuizException>(() => quiz.Publish());
    }

    [Test]
    public void Publish_GivenAlreadyPublishedQuiz_WhenCalled_ThenThrowsInvalidQuizException()
    {
        var quiz = CreatePublishedQuiz();

        Assert.Throws<InvalidQuizException>(() => quiz.Publish());
    }

    [Test]
    public void Unpublish_GivenPublishedQuiz_WhenCalled_ThenSetsStatusDraft()
    {
        var quiz = CreatePublishedQuiz();

        quiz.Unpublish();

        Assert.That(quiz.Status, Is.EqualTo(QuizStatus.Draft));
    }

    [Test]
    public void Unpublish_GivenDraftQuiz_WhenCalled_ThenThrowsInvalidQuizException()
    {
        var quiz = CreateQuiz();

        Assert.Throws<InvalidQuizException>(() => quiz.Unpublish());
    }

    [Test]
    public void Archive_GivenDraftQuiz_WhenCalled_ThenSetsStatusArchived()
    {
        var quiz = CreateQuiz();

        quiz.Archive();

        Assert.That(quiz.Status, Is.EqualTo(QuizStatus.Archived));
    }

    [Test]
    public void Archive_GivenPublishedQuiz_WhenCalled_ThenSetsStatusArchived()
    {
        var quiz = CreatePublishedQuiz();

        quiz.Archive();

        Assert.That(quiz.Status, Is.EqualTo(QuizStatus.Archived));
    }

    [Test]
    public void Archive_GivenAlreadyArchivedQuiz_WhenCalled_ThenThrowsInvalidQuizException()
    {
        var quiz = CreateQuiz();
        quiz.Archive();

        Assert.Throws<InvalidQuizException>(() => quiz.Archive());
    }

    [Test]
    public void RecordChallengeOutcome_GivenFirstPassedOutcome_WhenCalled_ThenSetsChallengeCountOneAndPassRateHundred()
    {
        var quiz = CreatePublishedQuiz();

        quiz.RecordChallengeOutcome(passed: true);

        Assert.Multiple(() =>
        {
            Assert.That(quiz.ChallengeCount, Is.EqualTo(1));
            Assert.That(quiz.PassRate, Is.EqualTo(100m));
        });
    }

    [Test]
    public void RecordChallengeOutcome_GivenOnePassedThenOneFailed_WhenCalled_ThenPassRateIsFifty()
    {
        var quiz = CreatePublishedQuiz();

        quiz.RecordChallengeOutcome(passed: true);
        quiz.RecordChallengeOutcome(passed: false);

        Assert.Multiple(() =>
        {
            Assert.That(quiz.ChallengeCount, Is.EqualTo(2));
            Assert.That(quiz.PassRate, Is.EqualTo(50m));
        });
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

    [Test]
    public void TakeDown_GivenPublishedQuiz_WhenCalled_ThenSetsStatusAndTakedownFields()
    {
        var quiz = CreatePublishedQuiz();
        var adminId = Guid.NewGuid();
        var now = new DateTime(2026, 7, 24, 9, 0, 0, DateTimeKind.Utc);

        quiz.TakeDown(adminId, "inappropriate content", now);

        Assert.Multiple(() =>
        {
            Assert.That(quiz.Status, Is.EqualTo(QuizStatus.TakenDown));
            Assert.That(quiz.TakedownReason, Is.EqualTo("inappropriate content"));
            Assert.That(quiz.TakedownBy, Is.EqualTo(adminId));
            Assert.That(quiz.TakedownAt, Is.EqualTo(now));
        });
    }

    [Test]
    public void TakeDown_GivenAlreadyTakenDownQuiz_WhenCalled_ThenThrowsInvalidQuizException()
    {
        var quiz = CreatePublishedQuiz();
        quiz.TakeDown(Guid.NewGuid(), "reason", DateTime.UtcNow);

        Assert.Throws<InvalidQuizException>(() => quiz.TakeDown(Guid.NewGuid(), "another reason", DateTime.UtcNow));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void TakeDown_GivenEmptyOrWhitespaceReason_WhenCalled_ThenThrowsInvalidQuizException(string reason)
    {
        var quiz = CreatePublishedQuiz();

        Assert.Throws<InvalidQuizException>(() => quiz.TakeDown(Guid.NewGuid(), reason, DateTime.UtcNow));
    }

    [Test]
    public void RestoreFromTakedown_GivenTakenDownQuiz_WhenCalled_ThenSetsStatusToDraftAndClearsFields()
    {
        var quiz = CreatePublishedQuiz();
        quiz.TakeDown(Guid.NewGuid(), "reason", DateTime.UtcNow);

        quiz.RestoreFromTakedown();

        Assert.Multiple(() =>
        {
            Assert.That(quiz.Status, Is.EqualTo(QuizStatus.Draft));
            Assert.That(quiz.TakedownReason, Is.Null);
            Assert.That(quiz.TakedownBy, Is.Null);
            Assert.That(quiz.TakedownAt, Is.Null);
        });
    }

    [Test]
    public void RestoreFromTakedown_GivenQuizNotTakenDown_WhenCalled_ThenThrowsInvalidQuizException()
    {
        var quiz = CreatePublishedQuiz();

        Assert.Throws<InvalidQuizException>(() => quiz.RestoreFromTakedown());
    }

    [Test]
    public void Archive_GivenTakenDownQuiz_WhenCalled_ThenThrowsInvalidQuizException()
    {
        var quiz = CreatePublishedQuiz();
        quiz.TakeDown(Guid.NewGuid(), "reason", DateTime.UtcNow);

        Assert.Throws<InvalidQuizException>(() => quiz.Archive());
    }
}

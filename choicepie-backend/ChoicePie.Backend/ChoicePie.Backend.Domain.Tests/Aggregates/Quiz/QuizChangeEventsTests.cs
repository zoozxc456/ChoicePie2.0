using ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Events;
using Difficulty = ChoicePie.Backend.Domain.Aggregates.Quiz.Enums.Difficulty;
using QuizAggregate = ChoicePie.Backend.Domain.Aggregates.Quiz.Quiz;
using QuizStatus = ChoicePie.Backend.Domain.Aggregates.Quiz.Enums.QuizStatus;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.Quiz;

// 題庫內容或狀態改變時要發出事件，讓讀取端快取可以據此失效。
[TestFixture]
public class QuizChangeEventsTests
{
    private static readonly Guid CreatorId = Guid.NewGuid();

    private static QuizAggregate CreateQuiz()
    {
        var quiz = QuizAggregate.Create(CreatorId, "Kubernetes 101", "A quiz about k8s", null, "⚓", "primary",
            Difficulty.Beginner, ["Kubernetes"]);
        quiz.ClearDomainEvents();
        return quiz;
    }

    private static Question CreateQuestion() => Question.Create("2+2=?", ["1", "2", "3", "4"], 3, "basic math");

    private static QuizAggregate CreateQuizWithQuestion()
    {
        var quiz = CreateQuiz();
        quiz.AddQuestion(CreateQuestion());
        quiz.ClearDomainEvents();
        return quiz;
    }

    private static QuizAggregate CreatePublishedQuiz()
    {
        var quiz = CreateQuizWithQuestion();
        quiz.Publish();
        quiz.ClearDomainEvents();
        return quiz;
    }

    private static QuizAggregate CreateArchivedQuiz()
    {
        var quiz = CreateQuiz();
        quiz.Archive();
        quiz.ClearDomainEvents();
        return quiz;
    }

    private static QuizAggregate CreateTakenDownQuiz()
    {
        var quiz = CreateQuiz();
        quiz.TakeDown(Guid.NewGuid(), "spam", DateTime.UtcNow);
        quiz.ClearDomainEvents();
        return quiz;
    }

    [Test]
    public void UpdateDetails_GivenSameTags_WhenCalled_ThenRaisesDetailsUpdatedWithoutTagChange()
    {
        var quiz = CreateQuiz();

        quiz.UpdateDetails("New title", null, ["Kubernetes"]);

        var domainEvent = quiz.DomainEvents.OfType<QuizDetailsUpdatedDomainEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(domainEvent.QuizId, Is.EqualTo(quiz.Id));
            Assert.That(domainEvent.TagsChanged, Is.False);
        });
    }

    [Test]
    public void UpdateDetails_GivenDifferentTags_WhenCalled_ThenRaisesDetailsUpdatedWithTagChange()
    {
        var quiz = CreateQuiz();

        quiz.UpdateDetails("Kubernetes 101", null, ["Kubernetes", "DevOps"]);

        Assert.That(quiz.DomainEvents.OfType<QuizDetailsUpdatedDomainEvent>().Single().TagsChanged, Is.True);
    }

    [Test]
    public void UpdateCover_GivenEditableQuiz_WhenCalled_ThenRaisesCoverUpdated()
    {
        var quiz = CreateQuiz();

        quiz.UpdateCover(null, "🚀", "secondary");

        Assert.That(quiz.DomainEvents.OfType<QuizCoverUpdatedDomainEvent>().Single().QuizId, Is.EqualTo(quiz.Id));
    }

    [Test]
    public void AddQuestion_GivenDraftQuiz_WhenCalled_ThenRaisesQuestionsChanged()
    {
        var quiz = CreateQuiz();

        quiz.AddQuestion(CreateQuestion());

        Assert.That(quiz.DomainEvents.OfType<QuizQuestionsChangedDomainEvent>().Single().QuizId, Is.EqualTo(quiz.Id));
    }

    [Test]
    public void UpdateQuestion_GivenExistingQuestion_WhenCalled_ThenRaisesQuestionsChanged()
    {
        var quiz = CreateQuizWithQuestion();

        quiz.UpdateQuestion(quiz.Questions[0].Id, "1+1=?", ["1", "2", "3", "4"], 1, "basic");

        Assert.That(quiz.DomainEvents.OfType<QuizQuestionsChangedDomainEvent>().Single().QuizId, Is.EqualTo(quiz.Id));
    }

    [Test]
    public void RemoveQuestion_GivenExistingQuestion_WhenCalled_ThenRaisesQuestionsChanged()
    {
        var quiz = CreateQuizWithQuestion();

        quiz.RemoveQuestion(quiz.Questions[0].Id);

        Assert.That(quiz.DomainEvents.OfType<QuizQuestionsChangedDomainEvent>().Single().QuizId, Is.EqualTo(quiz.Id));
    }

    private static IEnumerable<TestCaseData> StatusTransitions()
    {
        yield return new TestCaseData(
                (Func<QuizAggregate>)CreateQuizWithQuestion, (Action<QuizAggregate>)(q => q.Publish()),
                QuizStatus.Draft, QuizStatus.Published)
            .SetName("Publish_GivenDraftQuiz_WhenCalled_ThenRaisesStatusChanged");
        yield return new TestCaseData(
                (Func<QuizAggregate>)CreatePublishedQuiz, (Action<QuizAggregate>)(q => q.Unpublish()),
                QuizStatus.Published, QuizStatus.Draft)
            .SetName("Unpublish_GivenPublishedQuiz_WhenCalled_ThenRaisesStatusChanged");
        yield return new TestCaseData(
                (Func<QuizAggregate>)CreatePublishedQuiz, (Action<QuizAggregate>)(q => q.Archive()),
                QuizStatus.Published, QuizStatus.Archived)
            .SetName("Archive_GivenPublishedQuiz_WhenCalled_ThenRaisesStatusChanged");
        yield return new TestCaseData(
                (Func<QuizAggregate>)CreateArchivedQuiz, (Action<QuizAggregate>)(q => q.Unarchive()),
                QuizStatus.Archived, QuizStatus.Draft)
            .SetName("Unarchive_GivenArchivedQuiz_WhenCalled_ThenRaisesStatusChanged");
        yield return new TestCaseData(
                (Func<QuizAggregate>)CreatePublishedQuiz, (Action<QuizAggregate>)(q => q.Delete(CreatorId)),
                QuizStatus.Published, QuizStatus.Deleted)
            .SetName("Delete_GivenPublishedQuiz_WhenCalled_ThenRaisesStatusChanged");
        yield return new TestCaseData(
                (Func<QuizAggregate>)CreatePublishedQuiz,
                (Action<QuizAggregate>)(q => q.TakeDown(Guid.NewGuid(), "spam", DateTime.UtcNow)),
                QuizStatus.Published, QuizStatus.TakenDown)
            .SetName("TakeDown_GivenPublishedQuiz_WhenCalled_ThenRaisesStatusChanged");
        yield return new TestCaseData(
                (Func<QuizAggregate>)CreateTakenDownQuiz, (Action<QuizAggregate>)(q => q.RestoreFromTakedown()),
                QuizStatus.TakenDown, QuizStatus.Draft)
            .SetName("RestoreFromTakedown_GivenTakenDownQuiz_WhenCalled_ThenRaisesStatusChanged");
    }

    [TestCaseSource(nameof(StatusTransitions))]
    public void StatusTransition_WhenCalled_ThenRaisesStatusChangedWithPreviousAndCurrentStatus(
        Func<QuizAggregate> arrange, Action<QuizAggregate> act, QuizStatus expectedPrevious, QuizStatus expectedCurrent)
    {
        var quiz = arrange();

        act(quiz);

        var domainEvent = quiz.DomainEvents.OfType<QuizStatusChangedDomainEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(domainEvent.QuizId, Is.EqualTo(quiz.Id));
            Assert.That(domainEvent.PreviousStatus, Is.EqualTo(expectedPrevious));
            Assert.That(domainEvent.CurrentStatus, Is.EqualTo(expectedCurrent));
        });
    }

    [Test]
    public void RecordChallengeOutcome_WhenCalled_ThenRaisesNoChangeEvent()
    {
        var quiz = CreatePublishedQuiz();

        quiz.RecordChallengeOutcome(passed: true);
        quiz.RecordShare();

        Assert.That(quiz.DomainEvents, Is.Empty);
    }
}

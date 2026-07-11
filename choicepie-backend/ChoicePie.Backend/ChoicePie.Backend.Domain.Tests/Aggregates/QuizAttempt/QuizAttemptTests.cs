using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Enums;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Events;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Exceptions;
using QuizAttemptAggregate = ChoicePie.Backend.Domain.Aggregates.QuizAttempt.QuizAttempt;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.QuizAttempt;

[TestFixture]
public class QuizAttemptTests
{
    private static readonly Guid QuizId = Guid.NewGuid();
    private static readonly Guid MemberId = Guid.NewGuid();
    private static readonly Guid Question1 = Guid.NewGuid();
    private static readonly Guid Question2 = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static QuizAttemptAggregate StartAttempt() =>
        QuizAttemptAggregate.Start(QuizId, MemberId, [Question1, Question2], Now);

    [Test]
    public void Start_GivenValidInput_WhenCalled_ThenCreatesAttemptInProgress()
    {
        var attempt = StartAttempt();

        Assert.Multiple(() =>
        {
            Assert.That(attempt.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(attempt.QuizId, Is.EqualTo(QuizId));
            Assert.That(attempt.MemberId, Is.EqualTo(MemberId));
            Assert.That(attempt.Status, Is.EqualTo(AttemptStatus.InProgress));
            Assert.That(attempt.StartedAt, Is.EqualTo(Now));
            Assert.That(attempt.CompletedAt, Is.Null);
            Assert.That(attempt.Score, Is.Null);
            Assert.That(attempt.Passed, Is.Null);
            Assert.That(attempt.ExpectedQuestionIds, Is.EqualTo(new[] { Question1, Question2 }));
            Assert.That(attempt.Answers, Is.Empty);
        });
    }

    [Test]
    public void Start_GivenNoQuestions_WhenCalled_ThenThrowsInvalidQuizAttemptException()
    {
        Assert.Throws<InvalidQuizAttemptException>(() =>
            QuizAttemptAggregate.Start(QuizId, MemberId, [], Now));
    }

    [Test]
    public void SubmitAnswer_GivenExpectedQuestion_WhenCalled_ThenRecordsAnswer()
    {
        var attempt = StartAttempt();

        attempt.SubmitAnswer(Question1, 2, Now);

        var answer = attempt.Answers.Single();
        Assert.Multiple(() =>
        {
            Assert.That(answer.QuestionId, Is.EqualTo(Question1));
            Assert.That(answer.SelectedOptionIndex, Is.EqualTo(2));
        });
    }

    [Test]
    public void SubmitAnswer_GivenSameQuestionTwice_WhenCalled_ThenReplacesPreviousAnswer()
    {
        var attempt = StartAttempt();

        attempt.SubmitAnswer(Question1, 0, Now);
        attempt.SubmitAnswer(Question1, 3, Now);

        Assert.Multiple(() =>
        {
            Assert.That(attempt.Answers, Has.Count.EqualTo(1));
            Assert.That(attempt.Answers[0].SelectedOptionIndex, Is.EqualTo(3));
        });
    }

    [Test]
    public void SubmitAnswer_GivenUnexpectedQuestion_WhenCalled_ThenThrowsInvalidQuizAttemptException()
    {
        var attempt = StartAttempt();

        Assert.Throws<InvalidQuizAttemptException>(() => attempt.SubmitAnswer(Guid.NewGuid(), 0, Now));
    }

    [Test]
    public void SubmitAnswer_GivenCompletedAttempt_WhenCalled_ThenThrowsInvalidQuizAttemptStateException()
    {
        var attempt = StartAttempt();
        attempt.Complete(new Dictionary<Guid, int> { [Question1] = 0, [Question2] = 0 }, Now);

        Assert.Throws<InvalidQuizAttemptStateException>(() => attempt.SubmitAnswer(Question1, 0, Now));
    }

    [Test]
    public void Complete_GivenAllAnswersCorrect_WhenCalled_ThenSetsFullScoreAndRaisesPassedEvent()
    {
        var attempt = StartAttempt();
        attempt.SubmitAnswer(Question1, 1, Now);
        attempt.SubmitAnswer(Question2, 2, Now);

        attempt.Complete(new Dictionary<Guid, int> { [Question1] = 1, [Question2] = 2 }, Now);

        Assert.Multiple(() =>
        {
            Assert.That(attempt.Status, Is.EqualTo(AttemptStatus.Completed));
            Assert.That(attempt.CompletedAt, Is.EqualTo(Now));
            Assert.That(attempt.Score, Is.EqualTo(100m));
        });

        var domainEvent = attempt.DomainEvents.OfType<QuizAttemptCompletedDomainEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(domainEvent.AttemptId, Is.EqualTo(attempt.Id));
            Assert.That(domainEvent.QuizId, Is.EqualTo(QuizId));
            Assert.That(domainEvent.MemberId, Is.EqualTo(MemberId));
            Assert.That(domainEvent.Score, Is.EqualTo(100m));
            Assert.That(domainEvent.Passed, Is.True);
        });
    }

    [Test]
    public void Complete_GivenUnansweredQuestion_WhenCalled_ThenCountsItAsIncorrect()
    {
        var attempt = StartAttempt();
        attempt.SubmitAnswer(Question1, 1, Now);
        // Question2 left unanswered.

        attempt.Complete(new Dictionary<Guid, int> { [Question1] = 1, [Question2] = 2 }, Now);

        Assert.That(attempt.Score, Is.EqualTo(50m));
    }

    [Test]
    public void Complete_GivenScoreBelowPassThreshold_WhenCalled_ThenRaisesFailedEvent()
    {
        var attempt = StartAttempt();
        // Both answered wrong.
        attempt.SubmitAnswer(Question1, 0, Now);
        attempt.SubmitAnswer(Question2, 0, Now);

        attempt.Complete(new Dictionary<Guid, int> { [Question1] = 1, [Question2] = 1 }, Now);

        var domainEvent = attempt.DomainEvents.OfType<QuizAttemptCompletedDomainEvent>().Single();
        Assert.That(domainEvent.Passed, Is.False);
    }

    [Test]
    public void Complete_GivenScoreAtOrAbovePassThreshold_WhenCalled_ThenPassedIsTrue()
    {
        var attempt = StartAttempt();
        attempt.SubmitAnswer(Question1, 1, Now);
        attempt.SubmitAnswer(Question2, 2, Now);

        attempt.Complete(new Dictionary<Guid, int> { [Question1] = 1, [Question2] = 2 }, Now);

        Assert.That(attempt.Passed, Is.True);
    }

    [Test]
    public void Complete_GivenAlreadyCompletedAttempt_WhenCalled_ThenThrowsInvalidQuizAttemptStateException()
    {
        var attempt = StartAttempt();
        attempt.Complete(new Dictionary<Guid, int> { [Question1] = 0, [Question2] = 0 }, Now);

        Assert.Throws<InvalidQuizAttemptStateException>(() =>
            attempt.Complete(new Dictionary<Guid, int> { [Question1] = 0, [Question2] = 0 }, Now));
    }

    [Test]
    public void EnsureOwnedBy_GivenOwner_WhenCalled_ThenDoesNotThrow()
    {
        var attempt = StartAttempt();

        Assert.DoesNotThrow(() => attempt.EnsureOwnedBy(MemberId));
    }

    [Test]
    public void EnsureOwnedBy_GivenNonOwner_WhenCalled_ThenThrowsQuizAttemptForbiddenException()
    {
        var attempt = StartAttempt();

        Assert.Throws<QuizAttemptForbiddenException>(() => attempt.EnsureOwnedBy(Guid.NewGuid()));
    }
}

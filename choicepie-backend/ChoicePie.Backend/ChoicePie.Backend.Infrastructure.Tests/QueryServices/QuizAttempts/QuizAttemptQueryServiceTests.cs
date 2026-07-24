using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Infrastructure.QueryServices.QuizAttempts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using NSubstitute;
using QuizAttemptAggregate = ChoicePie.Backend.Domain.Aggregates.QuizAttempt.QuizAttempt;

namespace ChoicePie.Backend.Infrastructure.Tests.QueryServices.QuizAttempts;

[TestFixture]
public class QuizAttemptQueryServiceTests
{
    private IReadRepository _readRepository = null!;
    private QuizAttemptQueryService _sut = null!;
    private Member _member = null!;
    private Quiz _quiz = null!;
    private Question _question = null!;

    [SetUp]
    public void SetUp()
    {
        _readRepository = Substitute.For<IReadRepository>();
        _sut = new QuizAttemptQueryService(_readRepository);

        _member = Member.Create("Host Name");
        _quiz = Quiz.Create(_member.Id, "Kubernetes 101", null, "⚓", "g", Difficulty.Beginner, []);
        _question = Question.Create("2+2=?", ["1", "2", "3", "4"], 3, "basic math");
        _quiz.AddQuestion(_question);
        _quiz.Publish();

        _readRepository.Query<Quiz>().Returns(new List<Quiz> { _quiz }.AsQueryable());
    }

    [Test]
    public async Task GetByIdAsync_GivenCompletedAttempt_WhenCalled_ThenReturnsResultWithPerQuestionCorrectness()
    {
        var attempt = QuizAttemptAggregate.Start(_quiz.Id, _member.Id, [_question.Id], DateTime.UtcNow);
        attempt.SubmitAnswer(_question.Id, 3, DateTime.UtcNow);
        attempt.Complete(new Dictionary<Guid, int> { [_question.Id] = 3 }, DateTime.UtcNow);
        _readRepository.Query<QuizAttemptAggregate>().Returns(new List<QuizAttemptAggregate> { attempt }.AsQueryable());

        var result = await _sut.GetByIdAsync(attempt.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result!.QuizTitle, Is.EqualTo("Kubernetes 101"));
            Assert.That(result.MemberId, Is.EqualTo(_member.Id));
            Assert.That(result.Score, Is.EqualTo(100m));
            Assert.That(result.Answers.Single().IsCorrect, Is.True);
        });
    }

    [Test]
    public async Task GetByIdAsync_GivenInProgressAttempt_WhenCalled_ThenMasksCorrectAnswerAndExplanation()
    {
        var attempt = QuizAttemptAggregate.Start(_quiz.Id, _member.Id, [_question.Id], DateTime.UtcNow);
        attempt.SubmitAnswer(_question.Id, 1, DateTime.UtcNow);
        _readRepository.Query<QuizAttemptAggregate>().Returns(new List<QuizAttemptAggregate> { attempt }.AsQueryable());

        var result = await _sut.GetByIdAsync(attempt.Id, CancellationToken.None);

        var answer = result!.Answers.Single();
        Assert.Multiple(() =>
        {
            Assert.That(answer.SelectedOptionIndex, Is.EqualTo(1));
            Assert.That(answer.CorrectOptionIndex, Is.Null);
            Assert.That(answer.IsCorrect, Is.False);
            Assert.That(answer.Explanation, Is.Null);
        });
    }

    [Test]
    public async Task GetByIdAsync_GivenUnknownId_WhenCalled_ThenReturnsNull()
    {
        _readRepository.Query<QuizAttemptAggregate>().Returns(new List<QuizAttemptAggregate>().AsQueryable());

        var result = await _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ListHistoryAsync_GivenCompletedAndInProgressAttempts_WhenCalled_ThenReturnsOnlyCompletedNewestFirst()
    {
        var older = QuizAttemptAggregate.Start(_quiz.Id, _member.Id, [_question.Id], new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        older.SubmitAnswer(_question.Id, 3, new DateTime(2026, 1, 1, 0, 0, 5, DateTimeKind.Utc));
        older.Complete(new Dictionary<Guid, int> { [_question.Id] = 3 }, new DateTime(2026, 1, 1, 0, 0, 10, DateTimeKind.Utc));

        var newer = QuizAttemptAggregate.Start(_quiz.Id, _member.Id, [_question.Id], new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc));
        newer.SubmitAnswer(_question.Id, 3, new DateTime(2026, 1, 2, 0, 0, 5, DateTimeKind.Utc));
        newer.Complete(new Dictionary<Guid, int> { [_question.Id] = 3 }, new DateTime(2026, 1, 2, 0, 0, 8, DateTimeKind.Utc));

        var inProgress = QuizAttemptAggregate.Start(_quiz.Id, _member.Id, [_question.Id], DateTime.UtcNow);

        _readRepository.Query<QuizAttemptAggregate>()
            .Returns(new List<QuizAttemptAggregate> { older, newer, inProgress }.AsQueryable());

        var result = await _sut.ListHistoryAsync(_quiz.Id, _member.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0]!.Id, Is.EqualTo(newer.Id));
            Assert.That(result[1]!.Id, Is.EqualTo(older.Id));
            Assert.That(result[1]!.DurationMs, Is.EqualTo(10000));
        });
    }

    [Test]
    public async Task ListHistoryAsync_GivenAnotherMembersAttempt_WhenCalled_ThenExcludesIt()
    {
        var otherMember = Member.Create("Other Member");
        var attempt = QuizAttemptAggregate.Start(_quiz.Id, otherMember.Id, [_question.Id], DateTime.UtcNow);
        attempt.SubmitAnswer(_question.Id, 3, DateTime.UtcNow);
        attempt.Complete(new Dictionary<Guid, int> { [_question.Id] = 3 }, DateTime.UtcNow);
        _readRepository.Query<QuizAttemptAggregate>().Returns(new List<QuizAttemptAggregate> { attempt }.AsQueryable());

        var result = await _sut.ListHistoryAsync(_quiz.Id, _member.Id, CancellationToken.None);

        Assert.That(result, Is.Empty);
    }
}

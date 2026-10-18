using ChoicePie.Backend.Application.Quizzes.EventHandlers;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Events;
using ChoicePie.Backend.Shared.Application.Events;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class RecordChallengeOutcomeHandlerTests
{
    private IQuizRepository _quizRepository = null!;
    private RecordChallengeOutcomeHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _quizRepository = Substitute.For<IQuizRepository>();
        _sut = new RecordChallengeOutcomeHandler(_quizRepository);
    }

    // 很多人同時完成同一份題庫的挑戰時不能少算：交給 repository 用單一 UPDATE 原子更新次數與通過率。
    [TestCase(true)]
    [TestCase(false)]
    public async Task Handle_GivenCompletedAttempt_WhenCalled_ThenRecordsOutcomeAtomically(bool passed)
    {
        var quizId = Guid.NewGuid();
        var notification = new ReliableDomainEventNotification<QuizAttemptCompletedDomainEvent>(
            new QuizAttemptCompletedDomainEvent(Guid.NewGuid(), quizId, Guid.NewGuid(), passed ? 100m : 0m, passed));

        await _sut.Handle(notification, CancellationToken.None);

        await _quizRepository.Received(1).RecordChallengeOutcomeAsync(quizId, passed, Arg.Any<CancellationToken>());
    }
}

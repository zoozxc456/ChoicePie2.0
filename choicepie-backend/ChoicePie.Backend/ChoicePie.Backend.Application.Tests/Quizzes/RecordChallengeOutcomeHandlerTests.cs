using ChoicePie.Backend.Application.Quizzes.EventHandlers;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Events;
using ChoicePie.Backend.Shared.Application.Events;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class RecordChallengeOutcomeHandlerTests
{
    private IQuizRepository _quizRepository = null!;
    private IUnitOfWork _unitOfWork = null!;
    private RecordChallengeOutcomeHandler _sut = null!;
    private Quiz _quiz = null!;

    [SetUp]
    public void SetUp()
    {
        _quizRepository = Substitute.For<IQuizRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new RecordChallengeOutcomeHandler(_quizRepository, _unitOfWork);

        _quiz = Quiz.Create(Guid.NewGuid(), "Title", null, "⚓", "g", Difficulty.Beginner, []);
        _quizRepository.GetByIdAsync(_quiz.Id, Arg.Any<CancellationToken>()).Returns(_quiz);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenPassedAttempt_WhenCalled_ThenRecordsOutcomeAndPersists()
    {
        var notification = new DomainEventNotification<QuizAttemptCompletedDomainEvent>(
            new QuizAttemptCompletedDomainEvent(Guid.NewGuid(), _quiz.Id, Guid.NewGuid(), 100m, true));

        await _sut.Handle(notification, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(_quiz.ChallengeCount, Is.EqualTo(1));
            Assert.That(_quiz.PassRate, Is.EqualTo(100m));
        });
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

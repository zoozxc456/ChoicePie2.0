using ChoicePie.Backend.Application.QuizAttempts.Commands;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;
using QuizAttemptAggregate = ChoicePie.Backend.Domain.Aggregates.QuizAttempt.QuizAttempt;

namespace ChoicePie.Backend.Application.Tests.QuizAttempts;

[TestFixture]
public class CompleteQuizAttemptCommandHandlerTests
{
    private IQuizAttemptRepository _quizAttemptRepository = null!;
    private IQuizRepository _quizRepository = null!;
    private ICurrentUserService _currentUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private CompleteQuizAttemptCommandHandler _sut = null!;
    private readonly Guid _memberId = Guid.NewGuid();
    private Quiz _quiz = null!;
    private Question _question = null!;
    private QuizAttemptAggregate _attempt = null!;

    [SetUp]
    public void SetUp()
    {
        _quizAttemptRepository = Substitute.For<IQuizAttemptRepository>();
        _quizRepository = Substitute.For<IQuizRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new CompleteQuizAttemptCommandHandler(
            _quizAttemptRepository, _quizRepository, _currentUserService, _unitOfWork);

        _quiz = Quiz.Create(Guid.NewGuid(), "Title", null, "⚓", "g", Difficulty.Beginner, []);
        _question = Question.Create("2+2=?", ["1", "2", "3", "4"], 3, "basic math");
        _quiz.AddQuestion(_question);
        _quiz.Publish();
        _quizRepository.GetByIdAsync(_quiz.Id, Arg.Any<CancellationToken>()).Returns(_quiz);

        _attempt = QuizAttemptAggregate.Start(_quiz.Id, _memberId, [_question.Id], DateTime.UtcNow);
        _attempt.SubmitAnswer(_question.Id, 3, DateTime.UtcNow);
        _quizAttemptRepository.GetByIdAsync(_attempt.Id, Arg.Any<CancellationToken>()).Returns(_attempt);

        _currentUserService.UserId.Returns(_memberId);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenCorrectAnswer_WhenCalled_ThenCompletesWithFullScoreAndPersists()
    {
        var result = await _sut.Handle(new CompleteQuizAttemptCommand(_attempt.Id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Score, Is.EqualTo(100m));
            Assert.That(result.Passed, Is.True);
            Assert.That(result.Answers.Single().IsCorrect, Is.True);
            Assert.That(result.Answers.Single().CorrectOptionIndex, Is.EqualTo(3));
        });
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenNonOwner_WhenCalled_ThenThrowsQuizAttemptForbiddenException()
    {
        _currentUserService.UserId.Returns(Guid.NewGuid());

        Assert.ThrowsAsync<QuizAttemptForbiddenException>(() =>
            _sut.Handle(new CompleteQuizAttemptCommand(_attempt.Id), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenAttemptNotFound_WhenCalled_ThenThrowsQuizAttemptNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _quizAttemptRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((QuizAttemptAggregate?)null);

        Assert.ThrowsAsync<QuizAttemptNotFoundException>(() =>
            _sut.Handle(new CompleteQuizAttemptCommand(missingId), CancellationToken.None));
    }
}

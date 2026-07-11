using ChoicePie.Backend.Application.QuizAttempts.Commands;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;
using QuizAttemptAggregate = ChoicePie.Backend.Domain.Aggregates.QuizAttempt.QuizAttempt;

namespace ChoicePie.Backend.Application.Tests.QuizAttempts;

[TestFixture]
public class SubmitQuizAttemptAnswerCommandHandlerTests
{
    private IQuizAttemptRepository _quizAttemptRepository = null!;
    private ICurrentUserService _currentUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private SubmitQuizAttemptAnswerCommandHandler _sut = null!;
    private readonly Guid _memberId = Guid.NewGuid();
    private readonly Guid _questionId = Guid.NewGuid();
    private QuizAttemptAggregate _attempt = null!;

    [SetUp]
    public void SetUp()
    {
        _quizAttemptRepository = Substitute.For<IQuizAttemptRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new SubmitQuizAttemptAnswerCommandHandler(_quizAttemptRepository, _currentUserService, _unitOfWork);

        _attempt = QuizAttemptAggregate.Start(Guid.NewGuid(), _memberId, [_questionId], DateTime.UtcNow);
        _quizAttemptRepository.GetByIdAsync(_attempt.Id, Arg.Any<CancellationToken>()).Returns(_attempt);
        _currentUserService.UserId.Returns(_memberId);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenOwner_WhenCalled_ThenRecordsAnswerAndPersists()
    {
        await _sut.Handle(new SubmitQuizAttemptAnswerCommand(_attempt.Id, _questionId, 2), CancellationToken.None);

        Assert.That(_attempt.Answers.Single().SelectedOptionIndex, Is.EqualTo(2));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenNonOwner_WhenCalled_ThenThrowsQuizAttemptForbiddenException()
    {
        _currentUserService.UserId.Returns(Guid.NewGuid());

        Assert.ThrowsAsync<QuizAttemptForbiddenException>(() =>
            _sut.Handle(new SubmitQuizAttemptAnswerCommand(_attempt.Id, _questionId, 2), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenAttemptNotFound_WhenCalled_ThenThrowsQuizAttemptNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _quizAttemptRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((QuizAttemptAggregate?)null);

        Assert.ThrowsAsync<QuizAttemptNotFoundException>(() =>
            _sut.Handle(new SubmitQuizAttemptAnswerCommand(missingId, _questionId, 2), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenNoCurrentUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() =>
            _sut.Handle(new SubmitQuizAttemptAnswerCommand(_attempt.Id, _questionId, 2), CancellationToken.None));
    }
}

using ChoicePie.Backend.Application.QuizAttempts.Commands;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;
using QuizAttemptAggregate = ChoicePie.Backend.Domain.Aggregates.QuizAttempt.QuizAttempt;

namespace ChoicePie.Backend.Application.Tests.QuizAttempts;

[TestFixture]
public class StartQuizAttemptCommandHandlerTests
{
    private IQuizRepository _quizRepository = null!;
    private IQuizAttemptRepository _quizAttemptRepository = null!;
    private IMemberRepository _memberRepository = null!;
    private ICurrentUserService _currentUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private TimeProvider _timeProvider = null!;
    private StartQuizAttemptCommandHandler _sut = null!;
    private readonly Guid _memberId = Guid.NewGuid();
    private Quiz _quiz = null!;

    [SetUp]
    public void SetUp()
    {
        _quizRepository = Substitute.For<IQuizRepository>();
        _quizAttemptRepository = Substitute.For<IQuizAttemptRepository>();
        _memberRepository = Substitute.For<IMemberRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(DateTimeOffset.UtcNow);
        _sut = new StartQuizAttemptCommandHandler(
            _quizRepository, _quizAttemptRepository, _memberRepository, _currentUserService, _unitOfWork,
            _timeProvider);

        _quiz = Quiz.Create(Guid.NewGuid(), "Title", null, "⚓", "g", Difficulty.Beginner, []);
        _quiz.AddQuestion(Question.Create("2+2=?", ["1", "2", "3", "4"], 3, "basic math"));
        _quiz.Publish();
        _quizRepository.GetByIdAsync(_quiz.Id, Arg.Any<CancellationToken>()).Returns(_quiz);
        _currentUserService.UserId.Returns(_memberId);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenPublishedQuiz_WhenCalled_ThenStartsAttemptAndPersists()
    {
        var result = await _sut.Handle(new StartQuizAttemptCommand(_quiz.Id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.AttemptId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(result.Quiz.Id, Is.EqualTo(_quiz.Id));
            Assert.That(result.Quiz.Questions, Has.Count.EqualTo(1));
        });
        await _quizAttemptRepository.Received(1).AddAsync(
            Arg.Is<QuizAttemptAggregate>(a => a.QuizId == _quiz.Id && a.MemberId == _memberId),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenDraftQuiz_WhenCalled_ThenThrowsQuizNotPublishedException()
    {
        var draftQuiz = Quiz.Create(Guid.NewGuid(), "Draft", null, "⚓", "g", Difficulty.Beginner, []);
        _quizRepository.GetByIdAsync(draftQuiz.Id, Arg.Any<CancellationToken>()).Returns(draftQuiz);

        Assert.ThrowsAsync<QuizNotPublishedException>(() =>
            _sut.Handle(new StartQuizAttemptCommand(draftQuiz.Id), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenQuizNotFound_WhenCalled_ThenThrowsQuizNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _quizRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((Quiz?)null);

        Assert.ThrowsAsync<QuizNotFoundException>(() =>
            _sut.Handle(new StartQuizAttemptCommand(missingId), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenNoCurrentUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() =>
            _sut.Handle(new StartQuizAttemptCommand(_quiz.Id), CancellationToken.None));
    }
}

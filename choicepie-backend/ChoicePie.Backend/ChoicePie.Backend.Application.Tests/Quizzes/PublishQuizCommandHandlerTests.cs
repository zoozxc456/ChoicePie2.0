using ChoicePie.Backend.Application.Quizzes.Commands;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class PublishQuizCommandHandlerTests
{
    private IQuizRepository _quizRepository = null!;
    private IMemberRepository _memberRepository = null!;
    private ICurrentUserService _currentUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private PublishQuizCommandHandler _sut = null!;
    private readonly Guid _ownerId = Guid.NewGuid();
    private Quiz _quiz = null!;

    [SetUp]
    public void SetUp()
    {
        _quizRepository = Substitute.For<IQuizRepository>();
        _memberRepository = Substitute.For<IMemberRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new PublishQuizCommandHandler(_quizRepository, _memberRepository, _currentUserService, _unitOfWork);

        _quiz = Quiz.Create(_ownerId, "Title", null, "⚓", "g", Difficulty.Beginner, []);
        _quiz.AddQuestion(Question.Create("2+2=?", ["1", "2", "3", "4"], 3, "basic math"));
        _quizRepository.GetByIdAsync(_quiz.Id, Arg.Any<CancellationToken>()).Returns(_quiz);
        _currentUserService.UserId.Returns(_ownerId);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenOwner_WhenCalled_ThenPublishesAndPersists()
    {
        var result = await _sut.Handle(new PublishQuizCommand(_quiz.Id), CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo("published"));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenNonOwner_WhenCalled_ThenThrowsQuizForbiddenException()
    {
        _currentUserService.UserId.Returns(Guid.NewGuid());

        Assert.ThrowsAsync<QuizForbiddenException>(() => _sut.Handle(new PublishQuizCommand(_quiz.Id), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenQuizNotFound_WhenCalled_ThenThrowsQuizNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _quizRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((Quiz?)null);

        Assert.ThrowsAsync<QuizNotFoundException>(() => _sut.Handle(new PublishQuizCommand(missingId), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenNoCurrentUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() => _sut.Handle(new PublishQuizCommand(_quiz.Id), CancellationToken.None));
    }
}

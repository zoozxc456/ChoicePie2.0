using ChoicePie.Backend.Application.Quizzes.Commands;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class AddQuestionCommandHandlerTests
{
    private IQuizRepository _quizRepository = null!;
    private IMemberRepository _memberRepository = null!;
    private ICurrentUserService _currentUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private AddQuestionCommandHandler _sut = null!;
    private readonly Guid _ownerId = Guid.NewGuid();
    private Quiz _quiz = null!;

    [SetUp]
    public void SetUp()
    {
        _quizRepository = Substitute.For<IQuizRepository>();
        _memberRepository = Substitute.For<IMemberRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new AddQuestionCommandHandler(_quizRepository, _memberRepository, _currentUserService, _unitOfWork);

        _quiz = Quiz.Create(_ownerId, "Title", null, "⚓", "g", Difficulty.Beginner, []);
        _quizRepository.GetByIdAsync(_quiz.Id, Arg.Any<CancellationToken>()).Returns(_quiz);
        _currentUserService.UserId.Returns(_ownerId);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    private AddQuestionCommand ValidCommand() => new()
    {
        QuizId = _quiz.Id,
        Text = "2+2=?",
        Options = ["1", "2", "3", "4"],
        AnswerIndex = 3,
        Explanation = "basic math"
    };

    [Test]
    public async Task Handle_GivenOwner_WhenCalled_ThenAddsQuestionAndPersists()
    {
        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.That(result.QuestionCount, Is.EqualTo(1));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenNonOwner_WhenCalled_ThenThrowsQuizForbiddenException()
    {
        _currentUserService.UserId.Returns(Guid.NewGuid());

        Assert.ThrowsAsync<QuizForbiddenException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenQuizNotFound_WhenCalled_ThenThrowsQuizNotFoundException()
    {
        var command = ValidCommand() with { QuizId = Guid.NewGuid() };
        _quizRepository.GetByIdAsync(command.QuizId, Arg.Any<CancellationToken>()).Returns((Quiz?)null);

        Assert.ThrowsAsync<QuizNotFoundException>(() => _sut.Handle(command, CancellationToken.None));
    }
}

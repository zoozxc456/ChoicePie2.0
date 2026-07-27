using ChoicePie.Backend.Application.AdminQuizzes.Commands;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.AdminQuizzes;

[TestFixture]
public class AdminRestoreQuizCommandHandlerTests
{
    private IQuizRepository _quizRepository = null!;
    private IUnitOfWork _unitOfWork = null!;
    private AdminRestoreQuizCommandHandler _sut = null!;
    private Quiz _quiz = null!;

    [SetUp]
    public void SetUp()
    {
        _quizRepository = Substitute.For<IQuizRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new AdminRestoreQuizCommandHandler(_quizRepository, _unitOfWork);

        _quiz = Quiz.Create(Guid.NewGuid(), "Title", null, "⚓", "g", Difficulty.Beginner, []);
        _quiz.TakeDown(Guid.NewGuid(), "reason", DateTime.UtcNow);
        _quizRepository.GetByIdAsync(_quiz.Id, Arg.Any<CancellationToken>()).Returns(_quiz);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenTakenDownQuiz_WhenCalled_ThenRestoresAndPersists()
    {
        await _sut.Handle(new AdminRestoreQuizCommand(_quiz.Id), CancellationToken.None);

        Assert.That(_quiz.Status, Is.EqualTo(QuizStatus.Draft));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenQuizNotFound_WhenCalled_ThenThrowsQuizNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _quizRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((Quiz?)null);

        Assert.ThrowsAsync<QuizNotFoundException>(() =>
            _sut.Handle(new AdminRestoreQuizCommand(missingId), CancellationToken.None));
    }
}

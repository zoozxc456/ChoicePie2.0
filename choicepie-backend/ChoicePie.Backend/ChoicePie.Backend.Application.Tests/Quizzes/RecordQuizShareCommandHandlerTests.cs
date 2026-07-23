using ChoicePie.Backend.Application.Quizzes.Commands;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class RecordQuizShareCommandHandlerTests
{
    private IQuizRepository _quizRepository = null!;
    private IUnitOfWork _unitOfWork = null!;
    private RecordQuizShareCommandHandler _sut = null!;
    private Quiz _quiz = null!;

    [SetUp]
    public void SetUp()
    {
        _quizRepository = Substitute.For<IQuizRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new RecordQuizShareCommandHandler(_quizRepository, _unitOfWork);

        _quiz = Quiz.Create(Guid.NewGuid(), "Title", null, "⚓", "g", Difficulty.Beginner, []);
        _quizRepository.GetByIdAsync(_quiz.Id, Arg.Any<CancellationToken>()).Returns(_quiz);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_WhenCalled_ThenIncrementsShareCountAndPersists()
    {
        var result = await _sut.Handle(new RecordQuizShareCommand(_quiz.Id), CancellationToken.None);

        Assert.That(result, Is.EqualTo(1));
        Assert.That(_quiz.ShareCount, Is.EqualTo(1));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenQuizNotFound_WhenCalled_ThenThrowsQuizNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _quizRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((Quiz?)null);

        Assert.ThrowsAsync<QuizNotFoundException>(() => _sut.Handle(new RecordQuizShareCommand(missingId), CancellationToken.None));
    }
}

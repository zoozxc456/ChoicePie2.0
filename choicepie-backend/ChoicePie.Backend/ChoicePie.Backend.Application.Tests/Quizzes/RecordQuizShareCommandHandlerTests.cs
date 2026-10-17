using ChoicePie.Backend.Application.Quizzes.Commands;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class RecordQuizShareCommandHandlerTests
{
    private IQuizRepository _quizRepository = null!;
    private RecordQuizShareCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _quizRepository = Substitute.For<IQuizRepository>();
        _sut = new RecordQuizShareCommandHandler(_quizRepository);
    }

    // 同時分享時不能「讀出來加一再寫回」而少算：交給 repository 用單一 UPDATE 原子遞增。
    [Test]
    public async Task Handle_WhenCalled_ThenIncrementsShareCountAtomicallyAndReturnsNewCount()
    {
        var quizId = Guid.NewGuid();
        _quizRepository.IncrementShareCountAsync(quizId, Arg.Any<CancellationToken>()).Returns(7);

        var result = await _sut.Handle(new RecordQuizShareCommand(quizId), CancellationToken.None);

        Assert.That(result, Is.EqualTo(7));
    }

    [Test]
    public void Handle_GivenQuizNotFound_WhenCalled_ThenThrowsQuizNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _quizRepository.IncrementShareCountAsync(missingId, Arg.Any<CancellationToken>()).Returns((int?)null);

        Assert.ThrowsAsync<QuizNotFoundException>(() => _sut.Handle(new RecordQuizShareCommand(missingId), CancellationToken.None));
    }
}

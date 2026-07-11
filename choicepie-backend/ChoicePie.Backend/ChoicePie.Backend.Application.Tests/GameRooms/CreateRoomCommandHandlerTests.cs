using ChoicePie.Backend.Application.GameRooms.Commands;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.GameRooms;

[TestFixture]
public class CreateRoomCommandHandlerTests
{
    private IQuizRepository _quizRepository = null!;
    private IGameRoomRepository _gameRoomRepository = null!;
    private CreateRoomCommandHandler _sut = null!;
    private readonly Guid _hostUserId = Guid.NewGuid();
    private readonly Guid _quizId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _quizRepository = Substitute.For<IQuizRepository>();
        _gameRoomRepository = Substitute.For<IGameRoomRepository>();
        _sut = new CreateRoomCommandHandler(_quizRepository, _gameRoomRepository);
    }

    private Quiz CreateQuizWithQuestions(int count)
    {
        var quiz = Quiz.Create(_hostUserId, "Kubernetes 101", null, "⚓", "gradient", Difficulty.Beginner, []);
        for (var i = 0; i < count; i++)
        {
            quiz.AddQuestion(Question.Create($"Q{i}", ["1", "2", "3", "4"], 1, "explanation"));
        }

        return quiz;
    }

    private CreateRoomCommand ValidCommand() => new()
    {
        HostUserId = _hostUserId,
        QuizId = _quizId,
        QuestionIds = [],
        TimeLimitSeconds = 20
    };

    [Test]
    public async Task Handle_GivenExistingQuiz_WhenCalled_ThenSavesRoomWithAllQuestionsAndReturnsUniqueRoomCode()
    {
        var quiz = CreateQuizWithQuestions(3);
        _quizRepository.GetByIdAsync(_quizId, Arg.Any<CancellationToken>()).Returns(quiz);
        _gameRoomRepository.RoomCodeExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.That(result.RoomCode, Has.Length.EqualTo(6));

        await _gameRoomRepository.Received(1).SaveAsync(
            Arg.Is<Domain.Aggregates.GameRoom.GameRoom>(r =>
                r.HostUserId == _hostUserId && r.Questions.Count == 3 && r.RoomCode == result.RoomCode),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenGeneratedRoomCodeAlreadyTaken_WhenCalled_ThenRetriesUntilUnique()
    {
        var quiz = CreateQuizWithQuestions(1);
        _quizRepository.GetByIdAsync(_quizId, Arg.Any<CancellationToken>()).Returns(quiz);
        _gameRoomRepository.RoomCodeExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true, true, false);

        await _sut.Handle(ValidCommand(), CancellationToken.None);

        await _gameRoomRepository.Received(3).RoomCodeExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenUnknownQuiz_WhenCalled_ThenThrowsQuizNotFoundException()
    {
        _quizRepository.GetByIdAsync(_quizId, Arg.Any<CancellationToken>()).Returns((Quiz?)null);

        Assert.ThrowsAsync<QuizNotFoundException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }
}

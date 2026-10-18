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
    private TimeProvider _timeProvider = null!;
    private CreateRoomCommandHandler _sut = null!;
    private readonly Guid _hostUserId = Guid.NewGuid();
    private readonly Guid _quizId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _quizRepository = Substitute.For<IQuizRepository>();
        _gameRoomRepository = Substitute.For<IGameRoomRepository>();
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(DateTimeOffset.UtcNow);
        _gameRoomRepository.TryAddAsync(Arg.Any<Domain.Aggregates.GameRoom.GameRoom>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _sut = new CreateRoomCommandHandler(_quizRepository, _gameRoomRepository, _timeProvider);
    }

    private Quiz CreateQuizWithQuestions(int count)
    {
        var quiz = Quiz.Create(_hostUserId, "Kubernetes 101", null, null, "⚓", "primary", Difficulty.Beginner, []);
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
        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.That(result.RoomCode, Has.Length.EqualTo(6));

        await _gameRoomRepository.Received(1).TryAddAsync(
            Arg.Is<Domain.Aggregates.GameRoom.GameRoom>(r =>
                r.HostUserId == _hostUserId && r.Questions.Count == 3 && r.RoomCode == result.RoomCode),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenGeneratedRoomCodeAlreadyTaken_WhenCalled_ThenRetriesUntilAddSucceedsWithoutOverwriting()
    {
        var quiz = CreateQuizWithQuestions(1);
        _quizRepository.GetByIdAsync(_quizId, Arg.Any<CancellationToken>()).Returns(quiz);
        var attemptedCodes = new List<string>();
        _gameRoomRepository.TryAddAsync(Arg.Do<Domain.Aggregates.GameRoom.GameRoom>(r => attemptedCodes.Add(r.RoomCode)),
                Arg.Any<CancellationToken>())
            .Returns(false, false, true);

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.That(attemptedCodes, Has.Count.EqualTo(3));
        Assert.That(result.RoomCode, Is.EqualTo(attemptedCodes[^1]));
        await _gameRoomRepository.DidNotReceiveWithAnyArgs().SaveAsync(default!, default);
    }

    [Test]
    public void Handle_GivenUnknownQuiz_WhenCalled_ThenThrowsQuizNotFoundException()
    {
        _quizRepository.GetByIdAsync(_quizId, Arg.Any<CancellationToken>()).Returns((Quiz?)null);

        Assert.ThrowsAsync<QuizNotFoundException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }

    private Quiz CreateQuizOwnedBy(Guid ownerId)
    {
        var quiz = Quiz.Create(ownerId, "Someone else's quiz", null, null, "⚓", "primary", Difficulty.Beginner, []);
        quiz.AddQuestion(Question.Create("Q", ["1", "2", "3", "4"], 1, "explanation"));
        return quiz;
    }

    [Test]
    public void Handle_GivenOtherMembersDraftQuiz_WhenCalled_ThenThrowsQuizNotFoundAndSavesNoRoom()
    {
        _quizRepository.GetByIdAsync(_quizId, Arg.Any<CancellationToken>()).Returns(CreateQuizOwnedBy(Guid.NewGuid()));

        Assert.ThrowsAsync<QuizNotFoundException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));

        _gameRoomRepository.DidNotReceiveWithAnyArgs().TryAddAsync(default!, default);
    }

    [Test]
    public async Task Handle_GivenOtherMembersPublishedQuiz_WhenCalled_ThenSavesRoom()
    {
        var quiz = CreateQuizOwnedBy(Guid.NewGuid());
        quiz.Publish();
        _quizRepository.GetByIdAsync(_quizId, Arg.Any<CancellationToken>()).Returns(quiz);

        await _sut.Handle(ValidCommand(), CancellationToken.None);

        await _gameRoomRepository.ReceivedWithAnyArgs(1).TryAddAsync(default!, default);
    }

    [Test]
    public void Handle_GivenOtherMembersTakenDownQuiz_WhenCalled_ThenThrowsQuizNotFoundAndSavesNoRoom()
    {
        var quiz = CreateQuizOwnedBy(Guid.NewGuid());
        quiz.Publish();
        quiz.TakeDown(Guid.NewGuid(), "violation", DateTime.UtcNow);
        _quizRepository.GetByIdAsync(_quizId, Arg.Any<CancellationToken>()).Returns(quiz);

        Assert.ThrowsAsync<QuizNotFoundException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));

        _gameRoomRepository.DidNotReceiveWithAnyArgs().TryAddAsync(default!, default);
    }

    [Test]
    public void Handle_GivenOwnQuizTakenDown_WhenCalled_ThenThrowsQuizNotFoundAndSavesNoRoom()
    {
        var quiz = CreateQuizWithQuestions(1);
        quiz.Publish();
        quiz.TakeDown(Guid.NewGuid(), "violation", DateTime.UtcNow);
        _quizRepository.GetByIdAsync(_quizId, Arg.Any<CancellationToken>()).Returns(quiz);

        Assert.ThrowsAsync<QuizNotFoundException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));

        _gameRoomRepository.DidNotReceiveWithAnyArgs().TryAddAsync(default!, default);
    }

    [Test]
    public void Handle_GivenOwnQuizArchived_WhenCalled_ThenThrowsQuizNotFoundAndSavesNoRoom()
    {
        var quiz = CreateQuizWithQuestions(1);
        quiz.Archive();
        _quizRepository.GetByIdAsync(_quizId, Arg.Any<CancellationToken>()).Returns(quiz);

        Assert.ThrowsAsync<QuizNotFoundException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));

        _gameRoomRepository.DidNotReceiveWithAnyArgs().TryAddAsync(default!, default);
    }

    [Test]
    public async Task Handle_GivenHostConnectionId_WhenCalled_ThenSavedRoomTracksHostConnectionAsOnline()
    {
        _quizRepository.GetByIdAsync(_quizId, Arg.Any<CancellationToken>()).Returns(CreateQuizWithQuestions(1));
        var command = new CreateRoomCommand
        {
            HostUserId = _hostUserId,
            QuizId = _quizId,
            HostConnectionId = "host-conn"
        };

        await _sut.Handle(command, CancellationToken.None);

        await _gameRoomRepository.Received(1).TryAddAsync(
            Arg.Is<Domain.Aggregates.GameRoom.GameRoom>(r => r.HostConnectionId == "host-conn" && r.IsHostOnline),
            Arg.Any<CancellationToken>());
    }
}

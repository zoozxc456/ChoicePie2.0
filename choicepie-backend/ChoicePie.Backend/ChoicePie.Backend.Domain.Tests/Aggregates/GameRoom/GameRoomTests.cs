using ChoicePie.Backend.Domain.Aggregates.GameRoom.Enums;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.GameRoom;

[TestFixture]
public class GameRoomTests
{
    private static readonly Guid HostUserId = Guid.NewGuid();
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc);

    private static List<GameQuestionSnapshot> OneQuestion() =>
    [
        new GameQuestionSnapshot(Guid.NewGuid(), "1+1=?", ["1", "2", "3", "4"], AnswerIndex: 1, "基本加法")
    ];

    private static List<GameQuestionSnapshot> TwoQuestions() =>
    [
        new GameQuestionSnapshot(Guid.NewGuid(), "1+1=?", ["1", "2", "3", "4"], AnswerIndex: 1, "基本加法"),
        new GameQuestionSnapshot(Guid.NewGuid(), "2+2=?", ["1", "2", "3", "4"], AnswerIndex: 3, "基本加法")
    ];

    private static Domain.Aggregates.GameRoom.GameRoom CreateRoom(int timeLimitSeconds = 20) =>
        Domain.Aggregates.GameRoom.GameRoom.Create(HostUserId, "ABC123", Guid.NewGuid(), "測試題庫", "📝", "linear-gradient(135deg,#000,#111)", OneQuestion(), timeLimitSeconds, CreatedAtUtc);

    private static Domain.Aggregates.GameRoom.GameRoom CreateRoomWithTwoQuestions(int timeLimitSeconds = 20) =>
        Domain.Aggregates.GameRoom.GameRoom.Create(HostUserId, "ABC123", Guid.NewGuid(), "測試題庫", "📝", "linear-gradient(135deg,#000,#111)", TwoQuestions(), timeLimitSeconds, CreatedAtUtc);

    [Test]
    public void Create_GivenValidInput_WhenCalled_ThenFieldsAreSetAndPhaseIsLobby()
    {
        var room = CreateRoom();

        Assert.Multiple(() =>
        {
            Assert.That(room.HostUserId, Is.EqualTo(HostUserId));
            Assert.That(room.RoomCode, Is.EqualTo("ABC123"));
            Assert.That(room.TimeLimitSeconds, Is.EqualTo(20));
            Assert.That(room.Phase, Is.EqualTo(GamePhase.Lobby));
            Assert.That(room.CurrentQuestionIndex, Is.EqualTo(-1));
            Assert.That(room.Players, Is.Empty);
        });
    }

    [Test]
    public void Create_GivenEmptyQuestions_WhenCalled_ThenThrowsInvalidGameRoomException()
    {
        Assert.Throws<InvalidGameRoomException>(() =>
            Domain.Aggregates.GameRoom.GameRoom.Create(HostUserId, "ABC123", Guid.NewGuid(), "測試題庫", "📝", "linear-gradient(135deg,#000,#111)", [], 20, CreatedAtUtc));
    }

    [TestCase(5)]
    [TestCase(15)]
    [TestCase(0)]
    public void Create_GivenTimeLimitOutsideAllowedSet_WhenCalled_ThenThrowsInvalidTimeLimitException(int timeLimit)
    {
        Assert.Throws<InvalidTimeLimitException>(() => CreateRoom(timeLimit));
    }

    [Test]
    public void Join_GivenRoomInLobbyWithinPlayableWindow_WhenCalled_ThenAddsPlayerAndReturnsIt()
    {
        var room = CreateRoom();

        var player = room.Join("小明", "connection-1", CreatedAtUtc.AddMinutes(1));

        Assert.Multiple(() =>
        {
            Assert.That(room.Players, Has.Count.EqualTo(1));
            Assert.That(room.Players[0], Is.SameAs(player));
            Assert.That(player.Nickname, Is.EqualTo("小明"));
        });
    }

    [Test]
    public void Join_GivenRoomAlreadyAtCapacity_WhenCalled_ThenThrowsRoomFullException()
    {
        var room = CreateRoom();
        for (var i = 0; i < 30; i++)
        {
            room.Join($"player-{i}", $"connection-{i}", CreatedAtUtc.AddMinutes(1));
        }

        Assert.Throws<RoomFullException>(() => room.Join("overflow", "connection-31", CreatedAtUtc.AddMinutes(1)));
    }

    [Test]
    public void Join_GivenRoomPastTwentyFourHourPlayableWindow_WhenCalled_ThenThrowsRoomNotJoinableException()
    {
        var room = CreateRoom();

        Assert.Throws<RoomNotJoinableException>(() =>
            room.Join("小明", "connection-1", CreatedAtUtc.AddHours(24).AddSeconds(1)));
    }

    [Test]
    public void Join_GivenRoomAlreadyStarted_WhenCalled_ThenThrowsRoomAlreadyStartedException()
    {
        var room = CreateRoom();
        room.StartGame(CreatedAtUtc.AddSeconds(30));

        Assert.Throws<RoomAlreadyStartedException>(() =>
            room.Join("遲到玩家", "connection-late", CreatedAtUtc.AddMinutes(1)));
    }

    [Test]
    public void StartGame_GivenRoomInLobby_WhenCalled_ThenPhaseBecomesQuestionAndTimestampIsRecorded()
    {
        var room = CreateRoom();
        var startedAt = CreatedAtUtc.AddMinutes(1);

        room.StartGame(startedAt);

        Assert.Multiple(() =>
        {
            Assert.That(room.Phase, Is.EqualTo(GamePhase.Question));
            Assert.That(room.CurrentQuestionIndex, Is.EqualTo(0));
            Assert.That(room.CurrentQuestionStartedAtUtc, Is.EqualTo(startedAt));
        });
    }

    [Test]
    public void StartGame_GivenRoomAlreadyStarted_WhenCalled_ThenThrowsInvalidGamePhaseException()
    {
        var room = CreateRoom();
        room.StartGame(CreatedAtUtc.AddMinutes(1));

        Assert.Throws<InvalidGamePhaseException>(() => room.StartGame(CreatedAtUtc.AddMinutes(2)));
    }

    [Test]
    public void SubmitAnswer_GivenCorrectAnswerWithinThreeSeconds_WhenCalled_ThenReturnsFullScoreAndRecordsOnPlayer()
    {
        var room = CreateRoom();
        var startedAt = CreatedAtUtc.AddMinutes(1);
        var player = room.Join("小明", "connection-1", CreatedAtUtc.AddSeconds(30));
        room.StartGame(startedAt);

        var outcome = room.SubmitAnswer(player.Id, answerIndex: 1, startedAt.AddSeconds(2));

        Assert.Multiple(() =>
        {
            Assert.That(outcome.IsCorrect, Is.True);
            Assert.That(outcome.Score, Is.EqualTo(1000));
            Assert.That(outcome.CorrectAnswerIndex, Is.EqualTo(1));
            Assert.That(player.Score, Is.EqualTo(1000));
        });
    }

    [Test]
    public void SubmitAnswer_GivenWrongAnswer_WhenCalled_ThenReturnsZeroScore()
    {
        var room = CreateRoom();
        var startedAt = CreatedAtUtc.AddMinutes(1);
        var player = room.Join("小明", "connection-1", CreatedAtUtc.AddSeconds(30));
        room.StartGame(startedAt);

        var outcome = room.SubmitAnswer(player.Id, answerIndex: 0, startedAt.AddSeconds(2));

        Assert.That(outcome.IsCorrect, Is.False);
        Assert.That(outcome.Score, Is.EqualTo(0));
    }

    [Test]
    public void SubmitAnswer_GivenPlayerAlreadyAnsweredThisQuestion_WhenCalled_ThenThrowsAnswerAlreadySubmittedException()
    {
        var room = CreateRoom();
        var startedAt = CreatedAtUtc.AddMinutes(1);
        var player = room.Join("小明", "connection-1", CreatedAtUtc.AddSeconds(30));
        room.StartGame(startedAt);
        room.SubmitAnswer(player.Id, answerIndex: 1, startedAt.AddSeconds(2));

        Assert.Throws<AnswerAlreadySubmittedException>(() =>
            room.SubmitAnswer(player.Id, answerIndex: 1, startedAt.AddSeconds(3)));
    }

    [Test]
    public void SubmitAnswer_GivenRoomNotInQuestionPhase_WhenCalled_ThenThrowsInvalidGamePhaseException()
    {
        var room = CreateRoom();
        var player = room.Join("小明", "connection-1", CreatedAtUtc.AddMinutes(1));

        Assert.Throws<InvalidGamePhaseException>(() =>
            room.SubmitAnswer(player.Id, answerIndex: 1, CreatedAtUtc.AddMinutes(2)));
    }

    [Test]
    public void SubmitAnswer_GivenUnknownPlayerId_WhenCalled_ThenThrowsPlayerNotFoundException()
    {
        var room = CreateRoom();
        room.StartGame(CreatedAtUtc.AddMinutes(1));

        Assert.Throws<PlayerNotFoundException>(() =>
            room.SubmitAnswer(Guid.NewGuid(), answerIndex: 1, CreatedAtUtc.AddMinutes(2)));
    }

    [Test]
    public void EndCurrentQuestion_GivenRoomInQuestionPhase_WhenCalled_ThenPhaseBecomesRevealAndReturnsRankings()
    {
        var room = CreateRoom();
        var startedAt = CreatedAtUtc.AddMinutes(1);
        var player = room.Join("小明", "connection-1", CreatedAtUtc.AddSeconds(30));
        room.StartGame(startedAt);
        room.SubmitAnswer(player.Id, answerIndex: 1, startedAt.AddSeconds(2));

        var rankings = room.EndCurrentQuestion(startedAt.AddSeconds(20));

        Assert.Multiple(() =>
        {
            Assert.That(room.Phase, Is.EqualTo(GamePhase.Reveal));
            Assert.That(rankings, Has.Count.EqualTo(1));
            Assert.That(rankings[0].PlayerId, Is.EqualTo(player.Id));
            Assert.That(rankings[0].Score, Is.EqualTo(1000));
            Assert.That(rankings[0].Rank, Is.EqualTo(1));
        });
    }

    [Test]
    public void EndCurrentQuestion_GivenRoomNotInQuestionPhase_WhenCalled_ThenThrowsInvalidGamePhaseException()
    {
        var room = CreateRoom();

        Assert.Throws<InvalidGamePhaseException>(() => room.EndCurrentQuestion(CreatedAtUtc.AddMinutes(1)));
    }

    [Test]
    public void AdvanceToNextQuestion_GivenMoreQuestionsRemain_WhenCalled_ThenMovesToNextQuestionAndResetsPlayers()
    {
        var room = CreateRoomWithTwoQuestions();
        var startedAt = CreatedAtUtc.AddMinutes(1);
        var player = room.Join("小明", "connection-1", CreatedAtUtc.AddSeconds(30));
        room.StartGame(startedAt);
        room.SubmitAnswer(player.Id, answerIndex: 1, startedAt.AddSeconds(2));
        room.EndCurrentQuestion(startedAt.AddSeconds(20));

        var nextStartedAt = startedAt.AddSeconds(25);
        var ended = room.AdvanceToNextQuestion(nextStartedAt);

        Assert.Multiple(() =>
        {
            Assert.That(ended, Is.False);
            Assert.That(room.Phase, Is.EqualTo(GamePhase.Question));
            Assert.That(room.CurrentQuestionIndex, Is.EqualTo(1));
            Assert.That(room.CurrentQuestionStartedAtUtc, Is.EqualTo(nextStartedAt));
            Assert.That(player.SelectedOptionIndex, Is.Null);
            Assert.That(player.HasAnsweredQuestion(0), Is.True, "past answers must be preserved");
        });
    }

    [Test]
    public void AdvanceToNextQuestion_GivenLastQuestion_WhenCalled_ThenPhaseBecomesEndedAndRaisesGameEndedDomainEvent()
    {
        var room = CreateRoom();
        var startedAt = CreatedAtUtc.AddMinutes(1);
        var player = room.Join("小明", "connection-1", CreatedAtUtc.AddSeconds(30));
        room.StartGame(startedAt);
        room.SubmitAnswer(player.Id, answerIndex: 1, startedAt.AddSeconds(2));
        room.EndCurrentQuestion(startedAt.AddSeconds(20));

        var endedAt = startedAt.AddSeconds(25);
        var ended = room.AdvanceToNextQuestion(endedAt);

        Assert.Multiple(() =>
        {
            Assert.That(ended, Is.True);
            Assert.That(room.Phase, Is.EqualTo(GamePhase.Ended));
            var domainEvent = room.DomainEvents.OfType<Domain.Aggregates.GameRoom.Events.GameEndedDomainEvent>().Single();
            Assert.That(domainEvent.RoomId, Is.EqualTo(room.Id));
            Assert.That(domainEvent.RoomCode, Is.EqualTo(room.RoomCode));
            Assert.That(domainEvent.FinalRankings, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void AdvanceToNextQuestion_GivenRoomNotInRevealPhase_WhenCalled_ThenThrowsInvalidGamePhaseException()
    {
        var room = CreateRoom();

        Assert.Throws<InvalidGamePhaseException>(() => room.AdvanceToNextQuestion(CreatedAtUtc.AddMinutes(1)));
    }

    [Test]
    public void TogglePause_GivenRoomInQuestionPhase_WhenCalledTwice_ThenAccumulatesPausedDuration()
    {
        var room = CreateRoom();
        var startedAt = CreatedAtUtc.AddMinutes(1);
        var player = room.Join("小明", "connection-1", CreatedAtUtc.AddSeconds(30));
        room.StartGame(startedAt);

        room.TogglePause(startedAt.AddSeconds(2));
        room.TogglePause(startedAt.AddSeconds(7));

        // Paused for 5s, so answering 8s after question start should still land in the "<=3s elapsed" band.
        var outcome = room.SubmitAnswer(player.Id, answerIndex: 1, startedAt.AddSeconds(8));

        Assert.That(outcome.Score, Is.EqualTo(1000));
    }

    [Test]
    public void TogglePause_GivenRoomNotInQuestionPhase_WhenCalled_ThenThrowsInvalidGamePhaseException()
    {
        var room = CreateRoom();

        Assert.Throws<InvalidGamePhaseException>(() => room.TogglePause(CreatedAtUtc.AddMinutes(1)));
    }

    [Test]
    public void ToMemento_GivenRoomWithPlayersAndAnswers_WhenRestored_ThenStateIsEquivalent()
    {
        var room = CreateRoomWithTwoQuestions();
        var player = room.Join("小明", "connection-1", CreatedAtUtc.AddSeconds(30));
        room.StartGame(CreatedAtUtc.AddMinutes(1));
        room.SubmitAnswer(player.Id, answerIndex: 1, CreatedAtUtc.AddMinutes(1).AddSeconds(2));
        room.EndCurrentQuestion(CreatedAtUtc.AddMinutes(1).AddSeconds(5));

        var restored = Domain.Aggregates.GameRoom.GameRoom.Restore(room.ToMemento());

        Assert.Multiple(() =>
        {
            Assert.That(restored.Id, Is.EqualTo(room.Id));
            Assert.That(restored.HostUserId, Is.EqualTo(room.HostUserId));
            Assert.That(restored.RoomCode, Is.EqualTo(room.RoomCode));
            Assert.That(restored.TimeLimitSeconds, Is.EqualTo(room.TimeLimitSeconds));
            Assert.That(restored.Phase, Is.EqualTo(room.Phase));
            Assert.That(restored.CurrentQuestionIndex, Is.EqualTo(room.CurrentQuestionIndex));
            Assert.That(restored.Questions, Is.EqualTo(room.Questions));
            Assert.That(restored.Players, Has.Count.EqualTo(1));
            Assert.That(restored.Players[0].Id, Is.EqualTo(player.Id));
            Assert.That(restored.Players[0].Score, Is.EqualTo(player.Score));
            Assert.That(restored.Players[0].Answers, Is.EqualTo(player.Answers));
        });
    }
}

using ChoicePie.Backend.Application.GameRooms.Commands;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.GameRooms;

[TestFixture]
public class RejoinRoomCommandHandlerTests
{
    private IGameRoomRepository _gameRoomRepository = null!;
    private RejoinRoomCommandHandler _sut = null!;
    private readonly Guid _hostUserId = Guid.NewGuid();
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc);

    [SetUp]
    public void SetUp()
    {
        _gameRoomRepository = Substitute.For<IGameRoomRepository>();
        _sut = new RejoinRoomCommandHandler(_gameRoomRepository);
    }

    private Domain.Aggregates.GameRoom.GameRoom CreateRoom(int questionCount = 1)
    {
        var questions = Enumerable.Range(0, questionCount)
            .Select(i => new GameQuestionSnapshot(Guid.NewGuid(), $"Q{i}", ["1", "2", "3", "4"], 1, "解析"))
            .ToList();
        var room = Domain.Aggregates.GameRoom.GameRoom.Create(_hostUserId, "ABC123", questions, 20, CreatedAtUtc);
        room.Join("小明", "conn-1", CreatedAtUtc.AddSeconds(1));
        return room;
    }

    [Test]
    public async Task Handle_GivenRoomInLobby_WhenCalled_ThenReturnsWaitingStatusWithNoQuestionPayload()
    {
        var room = CreateRoom();
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);

        var result = await _sut.Handle(new RejoinRoomCommand("ABC123", _hostUserId), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Phase, Is.EqualTo("lobby"));
            Assert.That(result.Room.Status, Is.EqualTo("waiting"));
            Assert.That(result.Room.Players, Has.Count.EqualTo(1));
            Assert.That(result.CurrentQuestion, Is.Null);
            Assert.That(result.QuestionEnd, Is.Null);
            Assert.That(result.Rankings, Is.Null);
        });
    }

    [Test]
    public async Task Handle_GivenRoomInQuestionPhase_WhenCalled_ThenReturnsCurrentQuestionAndAnsweredCount()
    {
        var room = CreateRoom(2);
        room.StartGame(CreatedAtUtc.AddMinutes(1));
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);

        var result = await _sut.Handle(new RejoinRoomCommand("ABC123", _hostUserId), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Phase, Is.EqualTo("question"));
            Assert.That(result.Room.Status, Is.EqualTo("playing"));
            Assert.That(result.CurrentQuestion, Is.Not.Null);
            Assert.That(result.CurrentQuestion!.Index, Is.EqualTo(0));
            Assert.That(result.AnsweredCount, Is.EqualTo(0));
            Assert.That(result.TotalCount, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Handle_GivenRoomInRevealPhase_WhenCalled_ThenReturnsQuestionEndPayload()
    {
        var room = CreateRoom(2);
        room.StartGame(CreatedAtUtc.AddMinutes(1));
        room.EndCurrentQuestion(CreatedAtUtc.AddMinutes(1).AddSeconds(5));
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);

        var result = await _sut.Handle(new RejoinRoomCommand("ABC123", _hostUserId), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Phase, Is.EqualTo("reveal"));
            Assert.That(result.QuestionEnd, Is.Not.Null);
            Assert.That(result.QuestionEnd!.Rankings, Has.Count.EqualTo(1));
            Assert.That(result.CurrentQuestion, Is.Null);
        });
    }

    [Test]
    public async Task Handle_GivenRoomEnded_WhenCalled_ThenReturnsFinalRankings()
    {
        var room = CreateRoom(1);
        room.StartGame(CreatedAtUtc.AddMinutes(1));
        room.EndCurrentQuestion(CreatedAtUtc.AddMinutes(1).AddSeconds(5));
        room.AdvanceToNextQuestion(CreatedAtUtc.AddMinutes(1).AddSeconds(6));
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);

        var result = await _sut.Handle(new RejoinRoomCommand("ABC123", _hostUserId), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Phase, Is.EqualTo("ended"));
            Assert.That(result.Room.Status, Is.EqualTo("ended"));
            Assert.That(result.Rankings, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Handle_GivenUnknownRoomCode_WhenCalled_ThenThrowsRoomNotFoundException()
    {
        _gameRoomRepository.GetByRoomCodeAsync("ZZZZZZ", Arg.Any<CancellationToken>())
            .Returns((Domain.Aggregates.GameRoom.GameRoom?)null);

        var command = new RejoinRoomCommand("ZZZZZZ", _hostUserId);

        Assert.ThrowsAsync<RoomNotFoundException>(() => _sut.Handle(command, CancellationToken.None));
    }

    [Test]
    public void Handle_GivenCallerIsNotHost_WhenCalled_ThenThrowsRoomAccessDeniedException()
    {
        var room = CreateRoom();
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);

        var command = new RejoinRoomCommand("ABC123", Guid.NewGuid());

        Assert.ThrowsAsync<RoomAccessDeniedException>(() => _sut.Handle(command, CancellationToken.None));
    }
}

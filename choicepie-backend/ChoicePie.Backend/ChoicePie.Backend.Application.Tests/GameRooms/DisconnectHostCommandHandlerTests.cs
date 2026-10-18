using ChoicePie.Backend.Application.GameRooms;
using ChoicePie.Backend.Application.GameRooms.Commands;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Enums;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.GameRooms;

[TestFixture]
public class DisconnectHostCommandHandlerTests
{
    private IGameRoomRepository _gameRoomRepository = null!;
    private DisconnectHostCommandHandler _sut = null!;
    private readonly Guid _hostUserId = Guid.NewGuid();
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime StartedAtUtc = CreatedAtUtc.AddMinutes(1);

    [SetUp]
    public void SetUp()
    {
        _gameRoomRepository = Substitute.For<IGameRoomRepository>();
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(new DateTimeOffset(StartedAtUtc.AddSeconds(4)));
        _sut = new DisconnectHostCommandHandler(_gameRoomRepository, timeProvider);
    }

    private Domain.Aggregates.GameRoom.GameRoom CreateRoom(bool started)
    {
        var questions = new List<GameQuestionSnapshot>
        {
            new(Guid.NewGuid(), "1+1=?", ["1", "2", "3", "4"], AnswerIndex: 1, "基本加法")
        };
        var room = Domain.Aggregates.GameRoom.GameRoom.Create(_hostUserId, "ABC123", Guid.NewGuid(), "測試題庫", null, "📝", "linear-gradient(135deg,#000,#111)", questions, 20, CreatedAtUtc);
        room.AttachHostConnection(_hostUserId, "host-conn");
        if (started)
        {
            room.StartGame(_hostUserId, StartedAtUtc);
        }

        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);
        return room;
    }

    [Test]
    public async Task Handle_GivenQuestionInProgress_WhenHostConnectionDrops_ThenAutoPausesSavesAndReturnsPauseState()
    {
        var room = CreateRoom(started: true);

        var result = await _sut.Handle(new DisconnectHostCommand("ABC123", "host-conn"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Outcome, Is.EqualTo(HostDisconnectOutcome.MarkedOfflineAndPaused));
            Assert.That(result.Pause, Is.Not.Null);
            Assert.That(result.Pause!.IsPaused, Is.True);
            Assert.That(result.Pause.RemainingMs, Is.EqualTo(16_000));
            Assert.That(room.IsHostOnline, Is.False);
        });
        await _gameRoomRepository.Received(1).SaveAsync(room, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenLobby_WhenHostConnectionDrops_ThenMarksOfflineWithoutPauseState()
    {
        var room = CreateRoom(started: false);

        var result = await _sut.Handle(new DisconnectHostCommand("ABC123", "host-conn"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Outcome, Is.EqualTo(HostDisconnectOutcome.MarkedOffline));
            Assert.That(result.Pause, Is.Null);
            Assert.That(room.IsPaused, Is.False);
        });
        await _gameRoomRepository.Received(1).SaveAsync(room, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenStaleConnectionId_WhenCalled_ThenIgnoredAndNotSaved()
    {
        var room = CreateRoom(started: true);

        var result = await _sut.Handle(new DisconnectHostCommand("ABC123", "old-tab"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Outcome, Is.EqualTo(HostDisconnectOutcome.Ignored));
            Assert.That(room.IsHostOnline, Is.True);
            Assert.That(room.IsPaused, Is.False);
        });
        await _gameRoomRepository.DidNotReceive().SaveAsync(Arg.Any<Domain.Aggregates.GameRoom.GameRoom>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenRoomNoLongerExists_WhenCalled_ThenIgnored()
    {
        _gameRoomRepository.GetByRoomCodeAsync("ZZZZZZ", Arg.Any<CancellationToken>())
            .Returns((Domain.Aggregates.GameRoom.GameRoom?)null);

        var result = await _sut.Handle(new DisconnectHostCommand("ZZZZZZ", "x"), CancellationToken.None);

        Assert.That(result.Outcome, Is.EqualTo(HostDisconnectOutcome.Ignored));
    }
}

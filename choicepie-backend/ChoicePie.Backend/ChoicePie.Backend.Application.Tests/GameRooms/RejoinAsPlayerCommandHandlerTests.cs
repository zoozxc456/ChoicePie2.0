using ChoicePie.Backend.Application.GameRooms;
using ChoicePie.Backend.Application.GameRooms.Commands;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.GameRooms;

[TestFixture]
public class RejoinAsPlayerCommandHandlerTests
{
    private IGameRoomRepository _gameRoomRepository = null!;
    private RejoinAsPlayerCommandHandler _sut = null!;
    private readonly Guid _hostUserId = Guid.NewGuid();
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc);

    [SetUp]
    public void SetUp()
    {
        _gameRoomRepository = Substitute.For<IGameRoomRepository>();
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(new DateTimeOffset(CreatedAtUtc.AddMinutes(1).AddSeconds(5)));
        _sut = new RejoinAsPlayerCommandHandler(_gameRoomRepository, new GameRoomLock(), timeProvider);
    }

    private Domain.Aggregates.GameRoom.GameRoom CreateStartedRoom()
    {
        var questions = new List<GameQuestionSnapshot>
        {
            new(Guid.NewGuid(), "1+1=?", ["1", "2", "3", "4"], AnswerIndex: 1, "基本加法")
        };
        var room = Domain.Aggregates.GameRoom.GameRoom.Create(_hostUserId, "ABC123", Guid.NewGuid(), "測試題庫", null, "📝", "linear-gradient(135deg,#000,#111)", questions, 20, CreatedAtUtc);
        room.Join("小明", "conn-1", CreatedAtUtc.AddSeconds(1));
        room.Join("小華", "conn-2", CreatedAtUtc.AddSeconds(2));
        room.StartGame(_hostUserId, CreatedAtUtc.AddMinutes(1));
        return room;
    }

    [Test]
    public async Task Handle_GivenValidToken_WhenCalled_ThenReconnectsSavesAndReturnsSelfState()
    {
        var room = CreateStartedRoom();
        var player = room.Players[0];
        room.SubmitAnswer(player.Id, 1, CreatedAtUtc.AddMinutes(1).AddSeconds(1));
        room.DisconnectPlayer(player.Id, "conn-1");
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);

        var result = await _sut.Handle(new RejoinAsPlayerCommand("ABC123", player.ReconnectToken, "conn-9"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Player.Id, Is.EqualTo(player.Id));
            Assert.That(result.Player.IsConnected, Is.True);
            Assert.That(result.RoomState.Phase, Is.EqualTo("question"));
            Assert.That(result.RoomState.CurrentQuestion, Is.Not.Null);
            Assert.That(result.RoomState.RemainingMs, Is.EqualTo(15_000));
            Assert.That(result.RoomState.Self!.HasAnswered, Is.True);
            Assert.That(result.RoomState.Self.Score, Is.EqualTo(player.Score));
            Assert.That(result.RoomState.Self.LastAnswerResult!.IsCorrect, Is.True);
            Assert.That(result.RoomState.Room.Players.All(p => p.SelectedOptionIndex is null), Is.True);
        });
        await _gameRoomRepository.Received(1).SaveAsync(room, Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenUnknownToken_WhenCalled_ThenThrowsInvalidReconnectTokenException()
    {
        var room = CreateStartedRoom();
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);

        var command = new RejoinAsPlayerCommand("ABC123", "wrong", "conn-9");

        Assert.ThrowsAsync<InvalidReconnectTokenException>(() => _sut.Handle(command, CancellationToken.None));
    }

    [Test]
    public void Handle_GivenUnknownRoom_WhenCalled_ThenThrowsRoomNotFoundException()
    {
        _gameRoomRepository.GetByRoomCodeAsync("ZZZZZZ", Arg.Any<CancellationToken>())
            .Returns((Domain.Aggregates.GameRoom.GameRoom?)null);

        var command = new RejoinAsPlayerCommand("ZZZZZZ", "x", "conn-9");

        Assert.ThrowsAsync<RoomNotFoundException>(() => _sut.Handle(command, CancellationToken.None));
    }
}

using ChoicePie.Backend.Application.GameRooms;
using ChoicePie.Backend.Application.GameRooms.Commands;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Enums;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.GameRooms;

[TestFixture]
public class DisconnectPlayerCommandHandlerTests
{
    private IGameRoomRepository _gameRoomRepository = null!;
    private DisconnectPlayerCommandHandler _sut = null!;
    private readonly Guid _hostUserId = Guid.NewGuid();
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc);

    [SetUp]
    public void SetUp()
    {
        _gameRoomRepository = Substitute.For<IGameRoomRepository>();
        _sut = new DisconnectPlayerCommandHandler(_gameRoomRepository);
    }

    private Domain.Aggregates.GameRoom.GameRoom CreateLobbyRoom()
    {
        var questions = new List<GameQuestionSnapshot>
        {
            new(Guid.NewGuid(), "1+1=?", ["1", "2", "3", "4"], AnswerIndex: 1, "基本加法")
        };
        var room = Domain.Aggregates.GameRoom.GameRoom.Create(_hostUserId, "ABC123", Guid.NewGuid(), "測試題庫", null, "📝", "linear-gradient(135deg,#000,#111)", questions, 20, CreatedAtUtc);
        room.Join("小明", "conn-1", CreatedAtUtc.AddSeconds(1));
        return room;
    }

    [Test]
    public async Task Handle_GivenLobbyPhase_WhenCalled_ThenRemovesPlayerAndSaves()
    {
        var room = CreateLobbyRoom();
        var playerId = room.Players[0].Id;
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);

        var outcome = await _sut.Handle(new DisconnectPlayerCommand("ABC123", playerId, "conn-1"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(outcome, Is.EqualTo(PlayerDisconnectOutcome.Removed));
            Assert.That(room.Players, Is.Empty);
        });
        await _gameRoomRepository.Received(1).SaveAsync(room, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenGameStarted_WhenCalled_ThenMarksPlayerDisconnectedAndSaves()
    {
        var room = CreateLobbyRoom();
        var playerId = room.Players[0].Id;
        room.StartGame(_hostUserId, CreatedAtUtc.AddMinutes(1));
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);

        var outcome = await _sut.Handle(new DisconnectPlayerCommand("ABC123", playerId, "conn-1"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(outcome, Is.EqualTo(PlayerDisconnectOutcome.MarkedDisconnected));
            Assert.That(room.Players.Single().IsConnected, Is.False);
        });
        await _gameRoomRepository.Received(1).SaveAsync(room, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenStaleConnectionId_WhenCalled_ThenIgnoredAndNotSaved()
    {
        var room = CreateLobbyRoom();
        var player = room.Players[0];
        room.StartGame(_hostUserId, CreatedAtUtc.AddMinutes(1));
        room.ReconnectPlayer(player.ReconnectToken, "conn-2");
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);

        var outcome = await _sut.Handle(new DisconnectPlayerCommand("ABC123", player.Id, "conn-1"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(outcome, Is.EqualTo(PlayerDisconnectOutcome.Ignored));
            Assert.That(player.IsConnected, Is.True);
        });
        await _gameRoomRepository.DidNotReceive().SaveAsync(Arg.Any<Domain.Aggregates.GameRoom.GameRoom>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenRoomNoLongerExists_WhenCalled_ThenIgnored()
    {
        _gameRoomRepository.GetByRoomCodeAsync("ZZZZZZ", Arg.Any<CancellationToken>())
            .Returns((Domain.Aggregates.GameRoom.GameRoom?)null);

        var outcome = await _sut.Handle(new DisconnectPlayerCommand("ZZZZZZ", Guid.NewGuid(), "x"), CancellationToken.None);

        Assert.That(outcome, Is.EqualTo(PlayerDisconnectOutcome.Ignored));
    }
}

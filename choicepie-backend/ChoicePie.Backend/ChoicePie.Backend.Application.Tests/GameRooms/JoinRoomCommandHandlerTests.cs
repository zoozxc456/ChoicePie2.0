using ChoicePie.Backend.Application.GameRooms.Commands;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.GameRooms;

[TestFixture]
public class JoinRoomCommandHandlerTests
{
    private IGameRoomRepository _gameRoomRepository = null!;
    private JoinRoomCommandHandler _sut = null!;
    private readonly Guid _hostUserId = Guid.NewGuid();
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc);

    [SetUp]
    public void SetUp()
    {
        _gameRoomRepository = Substitute.For<IGameRoomRepository>();
        _sut = new JoinRoomCommandHandler(_gameRoomRepository);
    }

    private Domain.Aggregates.GameRoom.GameRoom CreateLobbyRoom()
    {
        var questions = new List<GameQuestionSnapshot>
        {
            new(Guid.NewGuid(), "1+1=?", ["1", "2", "3", "4"], AnswerIndex: 1, "基本加法")
        };
        return Domain.Aggregates.GameRoom.GameRoom.Create(_hostUserId, "ABC123", Guid.NewGuid(), "測試題庫", "📝", "linear-gradient(135deg,#000,#111)", questions, 20, CreatedAtUtc);
    }

    [Test]
    public async Task Handle_GivenRoomInLobby_WhenCalled_ThenJoinsPlayerAndSavesRoom()
    {
        var room = CreateLobbyRoom();
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);

        var result = await _sut.Handle(new JoinRoomCommand("ABC123", "小明", "conn-1"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Player.Nickname, Is.EqualTo("小明"));
            Assert.That(result.Player.Score, Is.EqualTo(0));
            Assert.That(result.Player.SelectedOptionIndex, Is.Null);
            Assert.That(result.RoomState.Phase, Is.EqualTo("lobby"));
            Assert.That(result.RoomState.Room.Status, Is.EqualTo("waiting"));
        });
        await _gameRoomRepository.Received(1).SaveAsync(room, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenMemberId_WhenCalled_ThenJoinedPlayerCarriesMemberId()
    {
        var room = CreateLobbyRoom();
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);
        var memberId = Guid.NewGuid();

        await _sut.Handle(new JoinRoomCommand("ABC123", "小明", "conn-1", memberId), CancellationToken.None);

        Assert.That(room.Players.Single().MemberId, Is.EqualTo(memberId));
    }

    [Test]
    public void Handle_GivenUnknownRoomCode_WhenCalled_ThenThrowsRoomNotFoundException()
    {
        _gameRoomRepository.GetByRoomCodeAsync("ZZZZZZ", Arg.Any<CancellationToken>())
            .Returns((Domain.Aggregates.GameRoom.GameRoom?)null);

        var command = new JoinRoomCommand("ZZZZZZ", "小明", "conn-1");

        Assert.ThrowsAsync<RoomNotFoundException>(() => _sut.Handle(command, CancellationToken.None));
    }

    [Test]
    public void Handle_GivenRoomAlreadyStarted_WhenCalled_ThenThrowsRoomAlreadyStartedException()
    {
        var room = CreateLobbyRoom();
        room.StartGame(CreatedAtUtc.AddMinutes(1));
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);

        var command = new JoinRoomCommand("ABC123", "小明", "conn-1");

        Assert.ThrowsAsync<RoomAlreadyStartedException>(() => _sut.Handle(command, CancellationToken.None));
    }
}

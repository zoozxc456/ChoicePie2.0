using ChoicePie.Backend.Application.GameRooms.Commands;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.GameRooms;

[TestFixture]
public class PauseGameCommandHandlerTests
{
    private IGameRoomRepository _gameRoomRepository = null!;
    private PauseGameCommandHandler _sut = null!;
    private readonly Guid _hostUserId = Guid.NewGuid();
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc);

    [SetUp]
    public void SetUp()
    {
        _gameRoomRepository = Substitute.For<IGameRoomRepository>();
        _sut = new PauseGameCommandHandler(_gameRoomRepository);
    }

    private Domain.Aggregates.GameRoom.GameRoom CreateStartedRoom()
    {
        var questions = new List<GameQuestionSnapshot>
        {
            new(Guid.NewGuid(), "1+1=?", ["1", "2", "3", "4"], AnswerIndex: 1, "基本加法")
        };
        var room = Domain.Aggregates.GameRoom.GameRoom.Create(_hostUserId, "ABC123", questions, 20, CreatedAtUtc);
        room.StartGame(CreatedAtUtc.AddMinutes(1));
        return room;
    }

    [Test]
    public async Task Handle_GivenRunningQuestion_WhenCalledFirstTime_ThenPausesRoomSavesAndReturnsTrue()
    {
        var room = CreateStartedRoom();
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);

        var result = await _sut.Handle(new PauseGameCommand("ABC123", _hostUserId), CancellationToken.None);

        Assert.That(result, Is.True);
        Assert.That(room.PausedAtUtc, Is.Not.Null);
        await _gameRoomRepository.Received(1).SaveAsync(room, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenAlreadyPaused_WhenCalledAgain_ThenResumesRoomAndReturnsFalse()
    {
        var room = CreateStartedRoom();
        room.TogglePause(CreatedAtUtc.AddMinutes(1).AddSeconds(2));
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);

        var result = await _sut.Handle(new PauseGameCommand("ABC123", _hostUserId), CancellationToken.None);

        Assert.That(result, Is.False);
        Assert.That(room.PausedAtUtc, Is.Null);
    }

    [Test]
    public void Handle_GivenUnknownRoomCode_WhenCalled_ThenThrowsRoomNotFoundException()
    {
        _gameRoomRepository.GetByRoomCodeAsync("ZZZZZZ", Arg.Any<CancellationToken>())
            .Returns((Domain.Aggregates.GameRoom.GameRoom?)null);

        var command = new PauseGameCommand("ZZZZZZ", _hostUserId);

        Assert.ThrowsAsync<RoomNotFoundException>(() => _sut.Handle(command, CancellationToken.None));
    }

    [Test]
    public void Handle_GivenCallerIsNotHost_WhenCalled_ThenThrowsRoomAccessDeniedException()
    {
        var room = CreateStartedRoom();
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);

        var command = new PauseGameCommand("ABC123", Guid.NewGuid());

        Assert.ThrowsAsync<RoomAccessDeniedException>(() => _sut.Handle(command, CancellationToken.None));
    }
}

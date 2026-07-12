using ChoicePie.Backend.Application.GameRooms.Commands;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.GameRooms;

[TestFixture]
public class StartGameCommandHandlerTests
{
    private IGameRoomRepository _gameRoomRepository = null!;
    private StartGameCommandHandler _sut = null!;
    private readonly Guid _hostUserId = Guid.NewGuid();
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc);

    [SetUp]
    public void SetUp()
    {
        _gameRoomRepository = Substitute.For<IGameRoomRepository>();
        _sut = new StartGameCommandHandler(_gameRoomRepository);
    }

    private Domain.Aggregates.GameRoom.GameRoom CreateLobbyRoom()
    {
        var questions = new List<GameQuestionSnapshot>
        {
            new(Guid.NewGuid(), "1+1=?", ["1", "2", "3", "4"], AnswerIndex: 1, "基本加法"),
            new(Guid.NewGuid(), "2+2=?", ["1", "2", "3", "4"], AnswerIndex: 3, "基本加法")
        };
        return Domain.Aggregates.GameRoom.GameRoom.Create(_hostUserId, "ABC123", Guid.NewGuid(), "測試題庫", "📝", "linear-gradient(135deg,#000,#111)", questions, 20, CreatedAtUtc);
    }

    [Test]
    public async Task Handle_GivenHostAndRoomInLobby_WhenCalled_ThenStartsGameSavesRoomAndReturnsFirstQuestion()
    {
        var room = CreateLobbyRoom();
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);

        var result = await _sut.Handle(new StartGameCommand("ABC123", _hostUserId), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Index, Is.EqualTo(0));
            Assert.That(result.Total, Is.EqualTo(2));
            Assert.That(result.Text, Is.EqualTo("1+1=?"));
            Assert.That(result.TimeLimit, Is.EqualTo(20));
        });
        await _gameRoomRepository.Received(1).SaveAsync(room, Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenUnknownRoomCode_WhenCalled_ThenThrowsRoomNotFoundException()
    {
        _gameRoomRepository.GetByRoomCodeAsync("ZZZZZZ", Arg.Any<CancellationToken>())
            .Returns((Domain.Aggregates.GameRoom.GameRoom?)null);

        var command = new StartGameCommand("ZZZZZZ", _hostUserId);

        Assert.ThrowsAsync<RoomNotFoundException>(() => _sut.Handle(command, CancellationToken.None));
    }

    [Test]
    public void Handle_GivenCallerIsNotHost_WhenCalled_ThenThrowsRoomAccessDeniedException()
    {
        var room = CreateLobbyRoom();
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);

        var command = new StartGameCommand("ABC123", Guid.NewGuid());

        Assert.ThrowsAsync<RoomAccessDeniedException>(() => _sut.Handle(command, CancellationToken.None));
    }

    [Test]
    public void Handle_GivenGameAlreadyStarted_WhenCalled_ThenThrowsInvalidGamePhaseException()
    {
        var room = CreateLobbyRoom();
        room.StartGame(CreatedAtUtc.AddMinutes(1));
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);

        var command = new StartGameCommand("ABC123", _hostUserId);

        Assert.ThrowsAsync<InvalidGamePhaseException>(() => _sut.Handle(command, CancellationToken.None));
    }
}

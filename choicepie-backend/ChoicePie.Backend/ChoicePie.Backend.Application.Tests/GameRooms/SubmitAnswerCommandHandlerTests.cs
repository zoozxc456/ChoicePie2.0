using ChoicePie.Backend.Application.GameRooms.Commands;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.GameRooms;

[TestFixture]
public class SubmitAnswerCommandHandlerTests
{
    private IGameRoomRepository _gameRoomRepository = null!;
    private SubmitAnswerCommandHandler _sut = null!;
    private readonly Guid _hostUserId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _gameRoomRepository = Substitute.For<IGameRoomRepository>();
        _sut = new SubmitAnswerCommandHandler(_gameRoomRepository);
    }

    private Domain.Aggregates.GameRoom.GameRoom CreateStartedRoom(DateTime createdAtUtc, DateTime startedAtUtc)
    {
        var questions = new List<GameQuestionSnapshot>
        {
            new(Guid.NewGuid(), "1+1=?", ["1", "2", "3", "4"], AnswerIndex: 1, "基本加法")
        };
        var room = Domain.Aggregates.GameRoom.GameRoom.Create(_hostUserId, "ABC123", questions, 20, createdAtUtc);
        room.Join("小明", "conn-1", createdAtUtc.AddSeconds(1));
        room.StartGame(startedAtUtc);
        return room;
    }

    [Test]
    public async Task Handle_GivenCorrectAnswer_WhenCalled_ThenSavesRoomAndReturnsScoreAndProgress()
    {
        var createdAt = new DateTime(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc);
        var startedAt = createdAt.AddSeconds(1);
        var room = CreateStartedRoom(createdAt, startedAt);
        var playerId = room.Players[0].Id;
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);

        var command = new SubmitAnswerCommand("ABC123", playerId, AnswerIndex: 1);
        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result.IsCorrect, Is.True);
            Assert.That(result.Result.CorrectAnswerIndex, Is.EqualTo(1));
            Assert.That(result.Progress.Answered, Is.EqualTo(1));
            Assert.That(result.Progress.Total, Is.EqualTo(1));
            Assert.That(result.Progress.PlayerId, Is.EqualTo(playerId));
        });
        await _gameRoomRepository.Received(1).SaveAsync(room, Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenUnknownRoomCode_WhenCalled_ThenThrowsRoomNotFoundException()
    {
        _gameRoomRepository.GetByRoomCodeAsync("ZZZZZZ", Arg.Any<CancellationToken>())
            .Returns((Domain.Aggregates.GameRoom.GameRoom?)null);

        var command = new SubmitAnswerCommand("ZZZZZZ", Guid.NewGuid(), 0);

        Assert.ThrowsAsync<RoomNotFoundException>(() => _sut.Handle(command, CancellationToken.None));
    }
}

using ChoicePie.Backend.Application.GameRooms.Queries;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.GameRooms;

[TestFixture]
public class WatchRoomQueryHandlerTests
{
    private IGameRoomRepository _gameRoomRepository = null!;
    private TimeProvider _timeProvider = null!;
    private WatchRoomQueryHandler _sut = null!;
    private readonly Guid _hostUserId = Guid.NewGuid();
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc);

    [SetUp]
    public void SetUp()
    {
        _gameRoomRepository = Substitute.For<IGameRoomRepository>();
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(CreatedAtUtc.AddMinutes(2)));
        _sut = new WatchRoomQueryHandler(_gameRoomRepository, _timeProvider);
    }

    private Domain.Aggregates.GameRoom.GameRoom CreateRoom()
    {
        var questions = new List<GameQuestionSnapshot>
        {
            new(Guid.NewGuid(), "1+1=?", ["1", "2", "3", "4"], AnswerIndex: 1, "基本加法"),
            new(Guid.NewGuid(), "2+2=?", ["1", "2", "3", "4"], AnswerIndex: 3, "基本加法")
        };
        var room = Domain.Aggregates.GameRoom.GameRoom.Create(_hostUserId, "ABC123", Guid.NewGuid(), "測試題庫", null, "📝", "linear-gradient(135deg,#000,#111)", questions, 20, CreatedAtUtc);
        room.Join("小明", "conn-1", CreatedAtUtc.AddSeconds(1));
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);
        return room;
    }

    [Test]
    public async Task Handle_GivenLobby_WhenWatching_ThenReturnsWaitingSyncWithoutSelfAndWithoutSpectatorAsPlayer()
    {
        CreateRoom();

        var result = await _sut.Handle(new WatchRoomQuery("ABC123"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Phase, Is.EqualTo("lobby"));
            Assert.That(result.Room.Players, Has.Count.EqualTo(1));
            Assert.That(result.Self, Is.Null);
            Assert.That(result.IsHostOnline, Is.True);
        });
    }

    [Test]
    public async Task Handle_GivenQuestionPhase_WhenWatching_ThenReturnsQuestionWithoutAnswerAndHidesSelectedOptions()
    {
        var room = CreateRoom();
        room.StartGame(_hostUserId, CreatedAtUtc.AddMinutes(1));
        room.SubmitAnswer(room.Players[0].Id, 1, CreatedAtUtc.AddMinutes(1).AddSeconds(3));

        var result = await _sut.Handle(new WatchRoomQuery("ABC123"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Phase, Is.EqualTo("question"));
            Assert.That(result.CurrentQuestion, Is.Not.Null);
            Assert.That(result.QuestionEnd, Is.Null);
            Assert.That(result.Room.Players.Single().SelectedOptionIndex, Is.Null);
            Assert.That(result.Self, Is.Null);
        });
    }

    [Test]
    public async Task Handle_GivenOfflineHost_WhenWatching_ThenSyncReportsHostOfflineAndPaused()
    {
        var room = CreateRoom();
        room.AttachHostConnection(_hostUserId, "host-conn");
        room.StartGame(_hostUserId, CreatedAtUtc.AddMinutes(1));
        room.DisconnectHost("host-conn", CreatedAtUtc.AddMinutes(1).AddSeconds(5));

        var result = await _sut.Handle(new WatchRoomQuery("ABC123"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsHostOnline, Is.False);
            Assert.That(result.IsPaused, Is.True);
        });
    }

    [Test]
    public async Task Handle_GivenRevealPhase_WhenWatching_ThenReturnsQuestionEndWithRankings()
    {
        var room = CreateRoom();
        room.StartGame(_hostUserId, CreatedAtUtc.AddMinutes(1));
        room.EndCurrentQuestion(_hostUserId, CreatedAtUtc.AddMinutes(1).AddSeconds(5));

        var result = await _sut.Handle(new WatchRoomQuery("ABC123"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Phase, Is.EqualTo("reveal"));
            Assert.That(result.QuestionEnd!.Rankings, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Handle_GivenUnknownRoom_WhenWatching_ThenThrowsRoomNotFoundException()
    {
        _gameRoomRepository.GetByRoomCodeAsync("ZZZZZZ", Arg.Any<CancellationToken>())
            .Returns((Domain.Aggregates.GameRoom.GameRoom?)null);

        Assert.ThrowsAsync<RoomNotFoundException>(() => _sut.Handle(new WatchRoomQuery("ZZZZZZ"), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenExpiredRoom_WhenWatching_ThenThrowsRoomNotJoinableException()
    {
        CreateRoom();
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(CreatedAtUtc.AddHours(49)));

        Assert.ThrowsAsync<RoomNotJoinableException>(() => _sut.Handle(new WatchRoomQuery("ABC123"), CancellationToken.None));
    }
}

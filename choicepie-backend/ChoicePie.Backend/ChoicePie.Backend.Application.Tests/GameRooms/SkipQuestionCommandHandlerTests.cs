using ChoicePie.Backend.Application.GameRooms.Commands;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;
using ChoicePie.Backend.Domain.Aggregates.GameSession;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using GameSessionAggregate = ChoicePie.Backend.Domain.Aggregates.GameSession.GameSession;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.GameRooms;

[TestFixture]
public class SkipQuestionCommandHandlerTests
{
    private IGameRoomRepository _gameRoomRepository = null!;
    private IGameSessionRepository _gameSessionRepository = null!;
    private IUnitOfWork _unitOfWork = null!;
    private SkipQuestionCommandHandler _sut = null!;
    private readonly Guid _hostUserId = Guid.NewGuid();
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc);

    [SetUp]
    public void SetUp()
    {
        _gameRoomRepository = Substitute.For<IGameRoomRepository>();
        _gameSessionRepository = Substitute.For<IGameSessionRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new SkipQuestionCommandHandler(_gameRoomRepository, _gameSessionRepository, _unitOfWork);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    private Domain.Aggregates.GameRoom.GameRoom CreateRoom(int questionCount)
    {
        var questions = Enumerable.Range(0, questionCount)
            .Select(i => new GameQuestionSnapshot(Guid.NewGuid(), $"Q{i}", ["1", "2", "3", "4"], 1, "解析"))
            .ToList();
        var room = Domain.Aggregates.GameRoom.GameRoom.Create(_hostUserId, "ABC123", Guid.NewGuid(), "測試題庫", null, "📝", "linear-gradient(135deg,#000,#111)", questions, 20, CreatedAtUtc);
        room.Join("小明", "conn-1", CreatedAtUtc.AddSeconds(1));
        return room;
    }

    [Test]
    public async Task Handle_GivenRoomInQuestionPhase_WhenCalled_ThenEndsQuestionWithoutPersistingSession()
    {
        var room = CreateRoom(2);
        room.StartGame(_hostUserId, CreatedAtUtc.AddMinutes(1));
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);

        var result = await _sut.Handle(new SkipQuestionCommand("ABC123", _hostUserId), CancellationToken.None);

        Assert.That(result.Kind, Is.EqualTo(SkipQuestionOutcomeKind.QuestionEnded));
        Assert.That(result.QuestionEnd, Is.Not.Null);
        await _gameSessionRepository.DidNotReceive().AddAsync(Arg.Any<GameSessionAggregate>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenLastQuestionInRevealPhase_WhenCalled_ThenPersistsGameSessionAndReturnsGameEnded()
    {
        var room = CreateRoom(1);
        room.StartGame(_hostUserId, CreatedAtUtc.AddMinutes(1));
        room.EndCurrentQuestion(_hostUserId, CreatedAtUtc.AddMinutes(1).AddSeconds(5));
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);

        var result = await _sut.Handle(new SkipQuestionCommand("ABC123", _hostUserId), CancellationToken.None);

        Assert.That(result.Kind, Is.EqualTo(SkipQuestionOutcomeKind.GameEnded));
        Assert.That(result.FinalRankings, Has.Count.EqualTo(1));
        await _gameSessionRepository.Received(1).AddAsync(
            Arg.Is<GameSessionAggregate>(s => s.RoomCode == "ABC123"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenSessionAlreadyRecordedButRoomStillInReveal_WhenCalled_ThenDoesNotRecordAgainAndMarksRoomEnded()
    {
        var room = CreateRoom(1);
        room.StartGame(_hostUserId, CreatedAtUtc.AddMinutes(1));
        room.EndCurrentQuestion(_hostUserId, CreatedAtUtc.AddMinutes(1).AddSeconds(5));
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);
        var alreadyRecorded = GameSessionAggregate.RecordFrom(CreateEndedCopyOf(room), CreatedAtUtc.AddMinutes(2));
        _gameSessionRepository.GetByIdAsync(room.Id, Arg.Any<CancellationToken>()).Returns(alreadyRecorded);

        var result = await _sut.Handle(new SkipQuestionCommand("ABC123", _hostUserId), CancellationToken.None);

        Assert.That(result.Kind, Is.EqualTo(SkipQuestionOutcomeKind.GameEnded));
        Assert.That(result.FinalRankings, Has.Count.EqualTo(1));
        await _gameSessionRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
        await _gameRoomRepository.Received(1).SaveAsync(
            Arg.Is<Domain.Aggregates.GameRoom.GameRoom>(r => r.Phase == Domain.Aggregates.GameRoom.Enums.GamePhase.Ended),
            Arg.Any<CancellationToken>());
    }

    /// <summary>模擬「上一次呼叫已寫入 Postgres」：從同一份 memento 還原出另一個實例推進到結束，不動到受測的 room。</summary>
    private Domain.Aggregates.GameRoom.GameRoom CreateEndedCopyOf(Domain.Aggregates.GameRoom.GameRoom room)
    {
        var copy = Domain.Aggregates.GameRoom.GameRoom.Restore(room.ToMemento());
        copy.AdvanceToNextQuestion(_hostUserId, CreatedAtUtc.AddMinutes(2));
        return copy;
    }

    [Test]
    public void Handle_GivenCallerIsNotHost_WhenCalled_ThenThrowsRoomAccessDeniedException()
    {
        var room = CreateRoom(1);
        room.StartGame(_hostUserId, CreatedAtUtc.AddMinutes(1));
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);

        var command = new SkipQuestionCommand("ABC123", Guid.NewGuid());

        Assert.ThrowsAsync<RoomAccessDeniedException>(() => _sut.Handle(command, CancellationToken.None));
    }
}

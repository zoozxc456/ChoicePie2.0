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
        var room = Domain.Aggregates.GameRoom.GameRoom.Create(_hostUserId, "ABC123", Guid.NewGuid(), "測試題庫", "📝", "linear-gradient(135deg,#000,#111)", questions, 20, CreatedAtUtc);
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
    public void Handle_GivenCallerIsNotHost_WhenCalled_ThenThrowsRoomAccessDeniedException()
    {
        var room = CreateRoom(1);
        room.StartGame(_hostUserId, CreatedAtUtc.AddMinutes(1));
        _gameRoomRepository.GetByRoomCodeAsync("ABC123", Arg.Any<CancellationToken>()).Returns(room);

        var command = new SkipQuestionCommand("ABC123", Guid.NewGuid());

        Assert.ThrowsAsync<RoomAccessDeniedException>(() => _sut.Handle(command, CancellationToken.None));
    }
}

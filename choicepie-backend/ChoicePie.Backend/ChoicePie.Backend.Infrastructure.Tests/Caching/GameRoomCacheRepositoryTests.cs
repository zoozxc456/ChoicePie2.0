using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;
using ChoicePie.Backend.Infrastructure.Caching.Repositories;
using ChoicePie.Backend.Shared.Application.Abstractions.Caching;
using ChoicePie.Backend.Shared.Application.Interfaces;
using NSubstitute;
using GameRoomAggregate = ChoicePie.Backend.Domain.Aggregates.GameRoom.GameRoom;
using GameRoomMemento = ChoicePie.Backend.Domain.Aggregates.GameRoom.GameRoomMemento;

namespace ChoicePie.Backend.Infrastructure.Tests.Caching;

[TestFixture]
public class GameRoomCacheRepositoryTests
{
    private static readonly Guid HostUserId = Guid.NewGuid();
    private static readonly Guid QuizId = Guid.NewGuid();
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc);

    private ICachingService _cachingService = null!;
    private GameRoomCacheRepository _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _cachingService = Substitute.For<ICachingService>();
        _sut = new GameRoomCacheRepository(_cachingService);
    }

    private static GameRoomAggregate BuildRoom(string roomCode = "ABC123")
    {
        var questions = new List<GameQuestionSnapshot>
        {
            new(Guid.NewGuid(), "1+1=?", ["1", "2", "3", "4"], AnswerIndex: 1, "e")
        };
        return GameRoomAggregate.Create(HostUserId, roomCode, QuizId, "測試題庫", "📝", "grad", questions, 20, CreatedAtUtc);
    }

    [Test]
    public async Task GetByRoomCodeAsync_GivenCachedRoom_WhenCalled_ThenReturnsRestoredRoomWithMatchingRoomCode()
    {
        var room = BuildRoom();
        _cachingService.GetAsync<GameRoomMemento>("GameRoom:ABC123", Arg.Any<CancellationToken>())
            .Returns(room.ToMemento());

        var result = await _sut.GetByRoomCodeAsync("ABC123");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.RoomCode, Is.EqualTo("ABC123"));
        Assert.That(result.HostUserId, Is.EqualTo(HostUserId));
    }

    [Test]
    public async Task GetByRoomCodeAsync_GivenNoCachedRoom_WhenCalled_ThenReturnsNull()
    {
        _cachingService.GetAsync<GameRoomMemento>("GameRoom:UNKNOWN", Arg.Any<CancellationToken>())
            .Returns((GameRoomMemento?)null);

        var result = await _sut.GetByRoomCodeAsync("UNKNOWN");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task SaveAsync_GivenRoom_WhenCalled_ThenPersistsUnderRoomCodeKeyWith48HourExpiration()
    {
        var room = BuildRoom();

        await _sut.SaveAsync(room);

        await _cachingService.Received(1).SetAsync(
            "GameRoom:ABC123",
            Arg.Is<GameRoomMemento>(m => m.RoomCode == "ABC123"),
            Arg.Is<ChoicePieCacheOptions>(o => o.Expiration == TimeSpan.FromHours(48)),
            tags: Arg.Any<IEnumerable<string>?>(),
            ct: Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task RoomCodeExistsAsync_GivenCachedRoom_WhenCalled_ThenReturnsTrue()
    {
        var room = BuildRoom();
        _cachingService.GetAsync<GameRoomMemento>("GameRoom:ABC123", Arg.Any<CancellationToken>())
            .Returns(room.ToMemento());

        var result = await _sut.RoomCodeExistsAsync("ABC123");

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task RoomCodeExistsAsync_GivenNoCachedRoom_WhenCalled_ThenReturnsFalse()
    {
        _cachingService.GetAsync<GameRoomMemento>("GameRoom:UNKNOWN", Arg.Any<CancellationToken>())
            .Returns((GameRoomMemento?)null);

        var result = await _sut.RoomCodeExistsAsync("UNKNOWN");

        Assert.That(result, Is.False);
    }
}

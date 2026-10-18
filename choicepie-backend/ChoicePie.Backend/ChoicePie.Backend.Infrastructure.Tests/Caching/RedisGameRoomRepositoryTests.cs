using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;
using ChoicePie.Backend.Infrastructure.Caching.Repositories;
using StackExchange.Redis;
using Testcontainers.Redis;
using GameRoomAggregate = ChoicePie.Backend.Domain.Aggregates.GameRoom.GameRoom;

namespace ChoicePie.Backend.Infrastructure.Tests.Caching;

/// <summary>
/// 兩個 repository 各自用獨立的 Redis 連線，代表兩台後端：一台寫入後另一台要立刻讀到，不能有本地快取延遲。
/// </summary>
[TestFixture]
public class RedisGameRoomRepositoryTests
{
    private static readonly Guid HostUserId = Guid.NewGuid();
    private static readonly Guid QuizId = Guid.NewGuid();
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc);

    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    private ConnectionMultiplexer _nodeAConnection = null!;
    private ConnectionMultiplexer _nodeBConnection = null!;
    private RedisGameRoomRepository _nodeA = null!;
    private RedisGameRoomRepository _nodeB = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await _redis.StartAsync();
        _nodeAConnection = await ConnectionMultiplexer.ConnectAsync(_redis.GetConnectionString());
        _nodeBConnection = await ConnectionMultiplexer.ConnectAsync(_redis.GetConnectionString());
        _nodeA = new RedisGameRoomRepository(_nodeAConnection);
        _nodeB = new RedisGameRoomRepository(_nodeBConnection);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _nodeAConnection.DisposeAsync();
        await _nodeBConnection.DisposeAsync();
        await _redis.DisposeAsync();
    }

    private static GameRoomAggregate BuildRoom(string roomCode)
    {
        var questions = new List<GameQuestionSnapshot>
        {
            new(Guid.NewGuid(), "1+1=?", ["1", "2", "3", "4"], AnswerIndex: 1, "e")
        };
        return GameRoomAggregate.Create(HostUserId, roomCode, QuizId, "測試題庫", null, "📝", "primary", questions, 20, CreatedAtUtc);
    }

    private static string NewRoomCode() => Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();

    [Test]
    public async Task GetByRoomCodeAsync_GivenRoomSavedByOtherNode_WhenCalled_ThenReturnsRestoredRoom()
    {
        var roomCode = NewRoomCode();
        var room = BuildRoom(roomCode);
        room.Join("小明", "conn-1", CreatedAtUtc.AddSeconds(1));
        await _nodeA.SaveAsync(room);

        var result = await _nodeB.GetByRoomCodeAsync(roomCode);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.RoomCode, Is.EqualTo(roomCode));
        Assert.That(result.HostUserId, Is.EqualTo(HostUserId));
        Assert.That(result.Players.Select(p => p.Nickname), Is.EqualTo(new[] { "小明" }));
        Assert.That(result.Questions, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task GetByRoomCodeAsync_GivenNoRoom_WhenCalled_ThenReturnsNull()
    {
        var result = await _nodeA.GetByRoomCodeAsync(NewRoomCode());

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task SaveAsync_GivenRoomOverwrittenByOtherNode_WhenReadImmediately_ThenSeesLatestState()
    {
        var roomCode = NewRoomCode();
        await _nodeA.SaveAsync(BuildRoom(roomCode));
        _ = await _nodeA.GetByRoomCodeAsync(roomCode);

        var updated = (await _nodeB.GetByRoomCodeAsync(roomCode))!;
        updated.Join("小華", "conn-2", CreatedAtUtc.AddSeconds(2));
        await _nodeB.SaveAsync(updated);

        var result = await _nodeA.GetByRoomCodeAsync(roomCode);
        Assert.That(result!.Players.Select(p => p.Nickname), Is.EqualTo(new[] { "小華" }));
    }

    [Test]
    public async Task SaveAsync_GivenRoom_WhenCalled_ThenExpiresAfter48Hours()
    {
        var roomCode = NewRoomCode();

        await _nodeA.SaveAsync(BuildRoom(roomCode));

        var ttl = await _nodeAConnection.GetDatabase().KeyTimeToLiveAsync($"ChoicePie:GameRoom:{roomCode}");
        Assert.That(ttl, Is.EqualTo(TimeSpan.FromHours(48)).Within(TimeSpan.FromMinutes(1)));
    }

    [Test]
    public async Task TryAddAsync_GivenUnusedRoomCode_WhenCalled_ThenStoresRoomWith48HourExpiryAndReturnsTrue()
    {
        var roomCode = NewRoomCode();

        var added = await _nodeA.TryAddAsync(BuildRoom(roomCode));

        Assert.That(added, Is.True);
        Assert.That(await _nodeB.GetByRoomCodeAsync(roomCode), Is.Not.Null);
        var ttl = await _nodeAConnection.GetDatabase().KeyTimeToLiveAsync($"ChoicePie:GameRoom:{roomCode}");
        Assert.That(ttl, Is.EqualTo(TimeSpan.FromHours(48)).Within(TimeSpan.FromMinutes(1)));
    }

    [Test]
    public async Task TryAddAsync_GivenRoomCodeTakenByOtherNode_WhenCalled_ThenReturnsFalseAndKeepsExistingRoom()
    {
        var roomCode = NewRoomCode();
        var existing = BuildRoom(roomCode);
        existing.Join("小明", "conn-1", CreatedAtUtc.AddSeconds(1));
        await _nodeA.TryAddAsync(existing);

        var added = await _nodeB.TryAddAsync(BuildRoom(roomCode));

        Assert.That(added, Is.False);
        var stored = await _nodeA.GetByRoomCodeAsync(roomCode);
        Assert.That(stored!.Players.Select(p => p.Nickname), Is.EqualTo(new[] { "小明" }));
    }
}

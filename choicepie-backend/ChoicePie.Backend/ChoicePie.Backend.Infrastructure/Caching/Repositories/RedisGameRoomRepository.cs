using System.Text.Json;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using StackExchange.Redis;

namespace ChoicePie.Backend.Infrastructure.Caching.Repositories;

/// <summary>
/// GameRoom 屬於短生命週期的即時狀態，直接存在 Redis（透過 GameRoomMemento 序列化），
/// 而非 EF/Postgres，符合 24h 可玩 / 48h 自動清除的房間生命週期規則。
/// 刻意不走 HybridCache：房間是唯一的真實狀態而不是可重算的快取，任何本地快取都會讓其他節點讀到舊房間。
/// 並行寫入由 GameRoomLockBehavior 的分散式鎖序列化。
/// </summary>
public sealed class RedisGameRoomRepository(IConnectionMultiplexer redis) : IGameRoomRepository, IScopedDependency
{
    private static readonly TimeSpan RoomExpiration = TimeSpan.FromHours(48);

    public async Task<GameRoom?> GetByRoomCodeAsync(string roomCode, CancellationToken ct = default)
    {
        var json = await redis.GetDatabase().StringGetAsync(BuildKey(roomCode));
        if (json.IsNullOrEmpty)
        {
            return null;
        }

        var memento = JsonSerializer.Deserialize<GameRoomMemento>(json.ToString());
        return memento is null ? null : GameRoom.Restore(memento);
    }

    public Task SaveAsync(GameRoom room, CancellationToken ct = default) =>
        redis.GetDatabase().StringSetAsync(
            BuildKey(room.RoomCode), JsonSerializer.Serialize(room.ToMemento()), RoomExpiration);

    public Task<bool> TryAddAsync(GameRoom room, CancellationToken ct = default) =>
        redis.GetDatabase().StringSetAsync(
            BuildKey(room.RoomCode), JsonSerializer.Serialize(room.ToMemento()), RoomExpiration, When.NotExists);

    private static RedisKey BuildKey(string roomCode) => $"ChoicePie:GameRoom:{roomCode}";
}

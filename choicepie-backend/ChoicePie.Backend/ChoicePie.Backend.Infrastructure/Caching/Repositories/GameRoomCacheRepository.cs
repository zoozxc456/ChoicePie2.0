using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Shared.Application.Abstractions.Caching;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;

namespace ChoicePie.Backend.Infrastructure.Caching.Repositories;

/// <summary>
/// GameRoom 屬於短生命週期的即時狀態，以 Redis-backed 快取儲存（透過 GameRoomMemento 序列化），
/// 而非 EF/Postgres，符合 24h 可玩 / 48h 自動清除的房間生命週期規則。
/// </summary>
public sealed class GameRoomCacheRepository(ICachingService cachingService) : IGameRoomRepository, IScopedDependency
{
    private static readonly ChoicePieCacheOptions RoomCacheOptions = new()
    {
        Expiration = TimeSpan.FromHours(48),
        LocalCacheExpiration = TimeSpan.FromSeconds(5)
    };

    public async Task<Domain.Aggregates.GameRoom.GameRoom?> GetByRoomCodeAsync(string roomCode, CancellationToken ct = default)
    {
        var memento = await cachingService.GetAsync<GameRoomMemento>(BuildKey(roomCode), ct);
        return memento is null ? null : Domain.Aggregates.GameRoom.GameRoom.Restore(memento);
    }

    public Task SaveAsync(Domain.Aggregates.GameRoom.GameRoom room, CancellationToken ct = default) =>
        cachingService.SetAsync(BuildKey(room.RoomCode), room.ToMemento(), RoomCacheOptions, ct: ct).AsTask();

    public async Task<bool> RoomCodeExistsAsync(string roomCode, CancellationToken ct = default) =>
        await cachingService.GetAsync<GameRoomMemento>(BuildKey(roomCode), ct) is not null;

    private static string BuildKey(string roomCode) => $"GameRoom:{roomCode}";
}

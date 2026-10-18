namespace ChoicePie.Backend.Domain.Aggregates.GameRoom;

/// <summary>
/// GameRoom 是短生命週期的即時狀態（24h 可玩 / 48h 清除），不走 EF/Postgres，
/// 由 Infrastructure 以 Redis-backed 快取實作，因此不沿用 IRepository&lt;T&gt; 這一套 EF 導向的介面。
/// </summary>
public interface IGameRoomRepository
{
    Task<GameRoom?> GetByRoomCodeAsync(string roomCode, CancellationToken ct = default);

    Task SaveAsync(GameRoom room, CancellationToken ct = default);

    /// <summary>
    /// 只在房號尚未被使用時寫入新房間（原子操作），房號已存在時回傳 false 且不覆蓋既有房間。
    /// </summary>
    Task<bool> TryAddAsync(GameRoom room, CancellationToken ct = default);
}

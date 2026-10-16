using System.Collections.Concurrent;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;

namespace ChoicePie.Backend.Application.GameRooms;

/// <summary>
/// 房間狀態存在 Redis 快取，為「讀取 → 修改 → 整份寫回」，沒有樂觀並行控制。
/// 斷線與快速重連會幾乎同時改寫同一份房間，這裡用同程序內的每房間互斥鎖序列化這兩條路徑，
/// 避免舊連線的斷線處理覆蓋掉剛完成的重連。（單一後端執行個體假設；多執行個體需改為分散式鎖。）
/// </summary>
public sealed class GameRoomLock : ISingletonDependency
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public async Task<IDisposable> AcquireAsync(string roomCode, CancellationToken ct = default)
    {
        var semaphore = _locks.GetOrAdd(roomCode, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(ct);
        return new Releaser(semaphore);
    }

    private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
    {
        public void Dispose() => semaphore.Release();
    }
}

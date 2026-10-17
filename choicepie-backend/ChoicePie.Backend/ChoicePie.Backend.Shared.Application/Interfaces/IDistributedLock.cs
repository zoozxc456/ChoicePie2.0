namespace ChoicePie.Backend.Shared.Application.Interfaces;

/// <summary>
/// 跨後端執行個體的互斥鎖。拿到的 lease 在 DisposeAsync 時釋放；
/// lease 有存活上限，持有者當機時鎖會自動過期，不會永久卡住。
/// </summary>
public interface IDistributedLock
{
    /// <summary>等到取得鎖為止；等太久會丟 <see cref="TimeoutException"/>。</summary>
    Task<IAsyncDisposable> AcquireAsync(string resource, CancellationToken ct = default);
}

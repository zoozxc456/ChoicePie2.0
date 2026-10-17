namespace ChoicePie.Backend.Shared.Application.Interfaces;

/// <summary>
/// 跨後端執行個體的互斥鎖。拿到的 lease 在 DisposeAsync 時釋放；
/// lease 有存活上限，持有者當機時鎖會自動過期，不會永久卡住。
/// </summary>
public interface IDistributedLock
{
    /// <summary>等到取得鎖為止；等太久會丟 <see cref="TimeoutException"/>。</summary>
    Task<IAsyncDisposable> AcquireAsync(string resource, CancellationToken ct = default);

    /// <summary>
    /// 只試一次、不等待：已被別人持有就回傳 null。用在「同時只能有一個在進行」而不是「排隊依序做」的情境，
    /// 例如耗時的 AI 出題。<paramref name="leaseDuration"/> 必須涵蓋受保護工作的最長執行時間。
    /// </summary>
    Task<IAsyncDisposable?> TryAcquireAsync(string resource, TimeSpan leaseDuration, CancellationToken ct = default);
}

using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;

namespace ChoicePie.Backend.Domain.Aggregates.RefreshToken;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    /// <summary>用資料庫目前的值覆蓋已追蹤的 token（樂觀並行衝突後，重新判斷別的請求寫進去的狀態）。</summary>
    Task ReloadAsync(RefreshToken token, CancellationToken ct = default);
}

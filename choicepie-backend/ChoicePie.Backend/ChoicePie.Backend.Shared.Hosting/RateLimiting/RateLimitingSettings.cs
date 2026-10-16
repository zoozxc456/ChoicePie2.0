using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;

namespace ChoicePie.Backend.Shared.Hosting.RateLimiting;

/// <summary>
/// 速率限制設定（appsettings 的 <c>RateLimiting</c> 區段），皆為固定視窗：每個視窗內最多 PermitLimit 次請求。
/// </summary>
public class RateLimitingSettings : IAppSetting
{
    public static string SectionName => "RateLimiting";

    /// <summary>登入／註冊等認證端點，依 IP 計算。</summary>
    public RateLimitRule Auth { get; set; } = new() { PermitLimit = 10, WindowSeconds = 60 };

    /// <summary>會寄信的端點（忘記密碼、重寄驗證信）。</summary>
    public RateLimitRule Email { get; set; } = new() { PermitLimit = 5, WindowSeconds = 900 };

    /// <summary>AI 出題，依使用者計算。</summary>
    public RateLimitRule Ai { get; set; } = new() { PermitLimit = 5, WindowSeconds = 60 };

    /// <summary>一般寫入動作（留言、檢舉、收藏、追蹤…）。</summary>
    public RateLimitRule Write { get; set; } = new() { PermitLimit = 30, WindowSeconds = 60 };

    /// <summary>其餘所有端點的全域保底限制。</summary>
    public RateLimitRule Global { get; set; } = new() { PermitLimit = 300, WindowSeconds = 60 };
}

public class RateLimitRule
{
    public int PermitLimit { get; set; }

    public int WindowSeconds { get; set; }
}

namespace ChoicePie.Backend.Shared.Application.Abstractions.Caching;

public class ChoicePieCacheOptions
{
    private const int DefaultExpirationMinutes = 5;
    private const int DefaultLocalCacheExpirationMinutes = 1;

    /// <summary>
    /// 快取總生存時間 (L1 與 L2 的最終過期時間)
    /// </summary>
    public TimeSpan? Expiration { get; set; }

    /// <summary>
    /// 本地記憶體 (L1) 的過期時間
    /// 建議設得比 Expiration 短，以確保能定時同步分散式快取的更新
    /// </summary>
    public TimeSpan? LocalCacheExpiration { get; set; }

    /// <summary>
    /// 預設配置：5分鐘過期，L1 存 1 分鐘
    /// </summary>
    public static ChoicePieCacheOptions Default => new()
    {
        Expiration = TimeSpan.FromMinutes(DefaultExpirationMinutes),
        LocalCacheExpiration = TimeSpan.FromMinutes(DefaultLocalCacheExpirationMinutes)
    };
}
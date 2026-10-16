namespace ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;

public class CloudflareR2Settings : IAppSetting
{
    public static string SectionName => "CloudflareR2";

    public required string AccessKeyId { get; set; }

    public required string SecretAccessKey { get; set; }

    /// <summary>R2 S3 API endpoint，格式 https://&lt;account-id&gt;.r2.cloudflarestorage.com</summary>
    public required string Endpoint { get; set; }

    public required string BucketName { get; set; }

    /// <summary>物件的公開存取網址前綴（R2.dev 網址或自訂網域），組合出最終圖片 URL 用。</summary>
    public required string PublicBaseUrl { get; set; }
}

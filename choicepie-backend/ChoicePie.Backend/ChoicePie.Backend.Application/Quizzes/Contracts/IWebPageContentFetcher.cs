namespace ChoicePie.Backend.Application.Quizzes.Contracts;

/// <summary>
/// 抓取公開網頁並擷取純文字內容的 Port。實作必須自行負責 SSRF 防護。
/// 失敗時丟出 Quiz 聚合的 WebPage* DomainException。
/// </summary>
public interface IWebPageContentFetcher
{
    Task<WebPageContent> FetchAsync(Uri url, CancellationToken cancellationToken);
}

/// <param name="Url">最終（跟隨轉址後）的網址。</param>
/// <param name="Title">網頁標題；純文字內容或無標題時為 null。</param>
/// <param name="Text">擷取出的純文字。</param>
public sealed record WebPageContent(Uri Url, string? Title, string Text);

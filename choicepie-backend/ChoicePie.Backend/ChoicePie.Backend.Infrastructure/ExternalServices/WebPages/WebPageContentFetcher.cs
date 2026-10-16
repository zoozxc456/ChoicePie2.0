using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;

namespace ChoicePie.Backend.Infrastructure.ExternalServices.WebPages;

/// <summary>
/// 抓取公開網頁並擷取純文字。SSRF 防護分兩層：這裡驗證每一跳的網址，
/// 連線時的位址驗證由 <see cref="SsrfSafeConnector"/>（SocketsHttpHandler.ConnectCallback）負責。
/// 刻意不實作 DI marker interface，改由 Program.cs 以 typed HttpClient 註冊（Scrutor 會蓋掉它）。
/// HttpClient 必須搭配不自動轉址的 handler（見 <see cref="SsrfSafeConnector.CreateHandler"/>）。
/// </summary>
public sealed partial class WebPageContentFetcher : IWebPageContentFetcher
{
    public const int MaxRedirects = 3;
    public const int MaxBodyBytes = 2 * 1024 * 1024;
    public static readonly TimeSpan TotalTimeout = TimeSpan.FromSeconds(10);

    private const string UserAgent = "ChoicePieBot/1.0 (+https://choicepie.app; quiz content fetcher)";

    private static readonly string[] AllowedMediaTypes = ["text/html", "application/xhtml+xml", "text/plain"];

    static WebPageContentFetcher() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    private readonly HttpClient httpClient;

    public WebPageContentFetcher(HttpClient httpClient)
    {
        this.httpClient = httpClient;
        // 總逾時由 FetchAsync 的 CancellationTokenSource 控制（涵蓋轉址與串流讀取）。
        this.httpClient.Timeout = Timeout.InfiniteTimeSpan;
        this.httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
        this.httpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml;q=0.9,text/plain;q=0.8");
        this.httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("zh-TW,zh;q=0.9,en;q=0.8");
    }

    public async Task<WebPageContent> FetchAsync(Uri url, CancellationToken cancellationToken)
    {
        EnsureUrlAllowed(url, isRedirect: false);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TotalTimeout);

        try
        {
            return await FetchCoreAsync(url, timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new WebPageFetchTimeoutException();
        }
        catch (BlockedAddressException ex)
        {
            throw new WebPageAccessDeniedException(ex.Message);
        }
        catch (HttpRequestException ex) when (FindBlockedAddress(ex) is { } blocked)
        {
            throw new WebPageAccessDeniedException(blocked.Message);
        }
        catch (HttpRequestException ex)
        {
            throw new WebPageFetchFailedException(ex.Message, ex);
        }
        catch (IOException ex)
        {
            throw new WebPageFetchFailedException(ex.Message, ex);
        }
    }

    private async Task<WebPageContent> FetchCoreAsync(Uri url, CancellationToken cancellationToken)
    {
        var current = url;

        for (var redirects = 0; ; redirects++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, current);
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (IsRedirect(response.StatusCode))
            {
                if (redirects >= MaxRedirects)
                {
                    throw new WebPageFetchFailedException($"Too many redirects (>{MaxRedirects}).");
                }

                var location = response.Headers.Location
                               ?? throw new WebPageFetchFailedException("Redirect response without Location header.");
                current = location.IsAbsoluteUri ? location : new Uri(current, location);
                EnsureUrlAllowed(current, isRedirect: true);
                continue;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new WebPageFetchFailedException($"Server returned {(int)response.StatusCode}.");
            }

            return await ReadContentAsync(current, response, cancellationToken);
        }
    }

    private static async Task<WebPageContent> ReadContentAsync(
        Uri finalUrl, HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var contentType = response.Content.Headers.ContentType;
        var mediaType = contentType?.MediaType?.ToLowerInvariant();
        if (mediaType is null || !AllowedMediaTypes.Contains(mediaType))
        {
            throw new WebPageFetchFailedException($"Unsupported content type '{mediaType ?? "(none)"}'.");
        }

        var bytes = await ReadLimitedAsync(response.Content, cancellationToken);
        var html = Decode(bytes, contentType, isHtml: mediaType != "text/plain");

        var extracted = mediaType == "text/plain"
            ? WebPageHtmlTextExtractor.ExtractFromPlainText(html)
            : WebPageHtmlTextExtractor.ExtractFromHtml(html);

        return new WebPageContent(finalUrl, extracted.Title, extracted.Text);
    }

    private static async Task<byte[]> ReadLimitedAsync(HttpContent content, CancellationToken cancellationToken)
    {
        await using var stream = await content.ReadAsStreamAsync(cancellationToken);
        using var buffer = new MemoryStream();
        var chunk = new byte[16 * 1024];

        while (buffer.Length < MaxBodyBytes)
        {
            var toRead = (int)Math.Min(chunk.Length, MaxBodyBytes - buffer.Length);
            var read = await stream.ReadAsync(chunk.AsMemory(0, toRead), cancellationToken);
            if (read == 0) break;
            buffer.Write(chunk, 0, read);
        }

        return buffer.ToArray();
    }

    private static string Decode(byte[] bytes, MediaTypeHeaderValue? contentType, bool isHtml)
    {
        var encoding = ResolveEncoding(contentType?.CharSet)
                       ?? (isHtml ? SniffMetaCharset(bytes) : null)
                       ?? Encoding.UTF8;

        // StreamReader 會優先採用 BOM；沒有 BOM 才用上面決定的編碼。
        using var reader = new StreamReader(new MemoryStream(bytes), encoding, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }

    private static Encoding? SniffMetaCharset(byte[] bytes)
    {
        var head = Encoding.ASCII.GetString(bytes, 0, Math.Min(bytes.Length, 4096));
        var match = MetaCharset().Match(head);
        return match.Success ? ResolveEncoding(match.Groups[1].Value) : null;
    }

    private static Encoding? ResolveEncoding(string? charset)
    {
        if (string.IsNullOrWhiteSpace(charset)) return null;

        try
        {
            return Encoding.GetEncoding(charset.Trim('"', '\'', ' '));
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static void EnsureUrlAllowed(Uri url, bool isRedirect)
    {
        switch (WebPageUrlGuard.CheckUrl(url))
        {
            case WebPageUrlCheck.Allowed:
                return;
            case WebPageUrlCheck.Unsupported when !isRedirect:
                throw new UnsupportedWebPageUrlException($"URL not supported: {url}");
            default:
                throw new WebPageAccessDeniedException(
                    $"{(isRedirect ? "Redirect target" : "URL")} not allowed: {url}");
        }
    }

    private static BlockedAddressException? FindBlockedAddress(Exception ex)
    {
        for (var e = ex; e is not null; e = e.InnerException!)
        {
            if (e is BlockedAddressException blocked) return blocked;
            if (e.InnerException is null) break;
        }

        return null;
    }

    private static bool IsRedirect(HttpStatusCode status)
        => status is HttpStatusCode.MovedPermanently or HttpStatusCode.Found or HttpStatusCode.SeeOther
            or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect;

    [GeneratedRegex("""<meta[^>]+charset\s*=\s*["']?\s*([A-Za-z0-9_\-:.]+)""", RegexOptions.IgnoreCase)]
    private static partial Regex MetaCharset();
}

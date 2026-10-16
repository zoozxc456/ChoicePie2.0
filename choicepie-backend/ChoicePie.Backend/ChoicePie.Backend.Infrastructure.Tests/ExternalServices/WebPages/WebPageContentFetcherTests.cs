using System.Net;
using System.Text;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Infrastructure.ExternalServices.WebPages;
using ChoicePie.Backend.Infrastructure.Tests.TestHelpers;

namespace ChoicePie.Backend.Infrastructure.Tests.ExternalServices.WebPages;

[TestFixture]
public class WebPageContentFetcherTests
{
    private static readonly Uri PageUrl = new("https://example.com/post");

    private static WebPageContentFetcher CreateSut(
        Func<HttpRequestMessage, HttpResponseMessage> respond, out FakeHttpMessageHandler handler)
    {
        handler = new FakeHttpMessageHandler(respond);
        return new WebPageContentFetcher(new HttpClient(handler));
    }

    private static HttpResponseMessage Html(string html, string contentType = "text/html; charset=utf-8")
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Encoding.UTF8.GetBytes(html)) };
        response.Content.Headers.TryAddWithoutValidation("Content-Type", contentType);
        return response;
    }

    private static HttpResponseMessage Redirect(string location, HttpStatusCode status = HttpStatusCode.Found)
    {
        var response = new HttpResponseMessage(status);
        response.Headers.TryAddWithoutValidation("Location", location);
        return response;
    }

    [Test]
    public async Task FetchAsync_GivenHtmlPage_WhenCalled_ThenReturnsTitleTextAndSendsUserAgent()
    {
        var sut = CreateSut(_ => Html("<html><head><title>Hi</title></head><body><p>Hello world</p></body></html>"), out var handler);

        var result = await sut.FetchAsync(PageUrl, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.EqualTo("Hi"));
            Assert.That(result.Text, Is.EqualTo("Hi\n\nHello world"));
            Assert.That(result.Url, Is.EqualTo(PageUrl));
            Assert.That(handler.LastRequest!.Headers.UserAgent.ToString(), Does.StartWith("ChoicePieBot/"));
        });
    }

    [TestCase("text/html")]
    [TestCase("TEXT/HTML; charset=UTF-8")]
    [TestCase("application/xhtml+xml")]
    public async Task FetchAsync_GivenAllowedHtmlContentType_WhenCalled_ThenSucceeds(string contentType)
    {
        var sut = CreateSut(_ => Html("<body><p>ok</p></body>", contentType), out _);

        var result = await sut.FetchAsync(PageUrl, CancellationToken.None);

        Assert.That(result.Text, Is.EqualTo("ok"));
    }

    [Test]
    public async Task FetchAsync_GivenPlainText_WhenCalled_ThenReturnsNormalizedTextWithoutTitle()
    {
        var sut = CreateSut(_ => Html("  just   some text  ", "text/plain; charset=utf-8"), out _);

        var result = await sut.FetchAsync(PageUrl, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.Null);
            Assert.That(result.Text, Is.EqualTo("just some text"));
        });
    }

    [TestCase("application/pdf")]
    [TestCase("image/png")]
    [TestCase("application/json")]
    [TestCase("application/octet-stream")]
    public void FetchAsync_GivenDisallowedContentType_WhenCalled_ThenThrowsFetchFailed(string contentType)
    {
        var sut = CreateSut(_ => Html("%PDF-1.4", contentType), out _);

        Assert.ThrowsAsync<WebPageFetchFailedException>(() => sut.FetchAsync(PageUrl, CancellationToken.None));
    }

    [Test]
    public void FetchAsync_GivenMissingContentType_WhenCalled_ThenThrowsFetchFailed()
    {
        var sut = CreateSut(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("<p>x</p>") }
            .Also(r => r.Content.Headers.ContentType = null), out _);

        Assert.ThrowsAsync<WebPageFetchFailedException>(() => sut.FetchAsync(PageUrl, CancellationToken.None));
    }

    [TestCase(HttpStatusCode.NotFound)]
    [TestCase(HttpStatusCode.Forbidden)]
    [TestCase(HttpStatusCode.InternalServerError)]
    public void FetchAsync_GivenErrorStatus_WhenCalled_ThenThrowsFetchFailed(HttpStatusCode status)
    {
        var sut = CreateSut(_ => new HttpResponseMessage(status), out _);

        Assert.ThrowsAsync<WebPageFetchFailedException>(() => sut.FetchAsync(PageUrl, CancellationToken.None));
    }

    [Test]
    public async Task FetchAsync_GivenBodyLargerThanLimit_WhenCalled_ThenReadsOnlyUpToTwoMegabytes()
    {
        var read = new CountingStream(new MemoryStream(Encoding.UTF8.GetBytes("<body><p>" + new string('a', 5 * 1024 * 1024) + "</p></body>")));
        var sut = CreateSut(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(read) };
            response.Content.Headers.TryAddWithoutValidation("Content-Type", "text/html");
            return response;
        }, out _);

        var result = await sut.FetchAsync(PageUrl, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(read.TotalBytesRead, Is.EqualTo(WebPageContentFetcher.MaxBodyBytes));
            Assert.That(result.Text.Length, Is.LessThanOrEqualTo(WebPageContentFetcher.MaxBodyBytes));
            Assert.That(result.Text, Does.StartWith("aaaa"));
        });
    }

    [Test]
    public async Task FetchAsync_GivenCharsetInContentTypeHeader_WhenCalled_ThenDecodesWithDeclaredCharset()
    {
        var big5 = Encoding.GetEncoding("big5");
        var sut = CreateSut(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(big5.GetBytes("<body><p>繁體中文內容</p></body>"))
            };
            response.Content.Headers.TryAddWithoutValidation("Content-Type", "text/html; charset=big5");
            return response;
        }, out _);

        var result = await sut.FetchAsync(PageUrl, CancellationToken.None);

        Assert.That(result.Text, Is.EqualTo("繁體中文內容"));
    }

    [Test]
    public async Task FetchAsync_GivenCharsetOnlyInMetaTag_WhenCalled_ThenDecodesWithMetaCharset()
    {
        var big5 = Encoding.GetEncoding("big5");
        var sut = CreateSut(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(big5.GetBytes("<html><head><meta charset=\"big5\"></head><body><p>繁體中文內容</p></body></html>"))
            };
            response.Content.Headers.TryAddWithoutValidation("Content-Type", "text/html");
            return response;
        }, out _);

        var result = await sut.FetchAsync(PageUrl, CancellationToken.None);

        Assert.That(result.Text, Is.EqualTo("繁體中文內容"));
    }

    [Test]
    public async Task FetchAsync_GivenRedirects_WhenCalled_ThenFollowsManuallyAndReportsFinalUrl()
    {
        var sut = CreateSut(request => request.RequestUri!.AbsolutePath switch
        {
            "/post" => Redirect("/step2"),
            "/step2" => Redirect("https://other.example.org/final", HttpStatusCode.MovedPermanently),
            _ => Html("<body><p>done</p></body>")
        }, out var handler);

        var result = await sut.FetchAsync(PageUrl, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Url, Is.EqualTo(new Uri("https://other.example.org/final")));
            Assert.That(result.Text, Is.EqualTo("done"));
            Assert.That(handler.LastRequest!.RequestUri, Is.EqualTo(new Uri("https://other.example.org/final")));
        });
    }

    [Test]
    public void FetchAsync_GivenMoreThanThreeRedirects_WhenCalled_ThenThrowsFetchFailed()
    {
        var calls = 0;
        var sut = CreateSut(_ => Redirect($"/hop{++calls}"), out _);

        Assert.ThrowsAsync<WebPageFetchFailedException>(() => sut.FetchAsync(PageUrl, CancellationToken.None));
        Assert.That(calls, Is.EqualTo(WebPageContentFetcher.MaxRedirects + 1));
    }

    [TestCase("http://127.0.0.1/admin")]
    [TestCase("http://169.254.169.254/latest/meta-data/")]
    [TestCase("http://localhost/")]
    [TestCase("https://example.com:8000/")]
    [TestCase("https://user:pw@example.com/")]
    [TestCase("http://[::ffff:10.0.0.1]/")]
    public void FetchAsync_GivenRedirectToBlockedTarget_WhenCalled_ThenThrowsAccessDeniedWithoutFollowing(string target)
    {
        var requested = new List<Uri>();
        var sut = CreateSut(request =>
        {
            requested.Add(request.RequestUri!);
            return Redirect(target);
        }, out _);

        Assert.ThrowsAsync<WebPageAccessDeniedException>(() => sut.FetchAsync(PageUrl, CancellationToken.None));
        Assert.That(requested, Is.EqualTo(new[] { PageUrl }));
    }

    [Test]
    public void FetchAsync_GivenRedirectToNonHttpScheme_WhenCalled_ThenThrowsAccessDenied()
    {
        var sut = CreateSut(_ => Redirect("file:///etc/passwd"), out _);

        Assert.ThrowsAsync<WebPageAccessDeniedException>(() => sut.FetchAsync(PageUrl, CancellationToken.None));
    }

    [Test]
    public void FetchAsync_GivenRedirectWithoutLocation_WhenCalled_ThenThrowsFetchFailed()
    {
        var sut = CreateSut(_ => new HttpResponseMessage(HttpStatusCode.Found), out _);

        Assert.ThrowsAsync<WebPageFetchFailedException>(() => sut.FetchAsync(PageUrl, CancellationToken.None));
    }

    [TestCase("ftp://example.com/x")]
    [TestCase("file:///etc/passwd")]
    public void FetchAsync_GivenUnsupportedScheme_WhenCalled_ThenThrowsUnsupportedWithoutRequest(string url)
    {
        var sut = CreateSut(_ => Html("x"), out var handler);

        Assert.ThrowsAsync<UnsupportedWebPageUrlException>(() => sut.FetchAsync(new Uri(url), CancellationToken.None));
        Assert.That(handler.LastRequest, Is.Null);
    }

    [TestCase("http://127.0.0.1/")]
    [TestCase("http://10.1.2.3/")]
    [TestCase("http://169.254.169.254/")]
    [TestCase("https://example.com:8000/")]
    [TestCase("https://user:pw@example.com/")]
    public void FetchAsync_GivenBlockedInitialUrl_WhenCalled_ThenThrowsAccessDeniedWithoutRequest(string url)
    {
        var sut = CreateSut(_ => Html("x"), out var handler);

        Assert.ThrowsAsync<WebPageAccessDeniedException>(() => sut.FetchAsync(new Uri(url), CancellationToken.None));
        Assert.That(handler.LastRequest, Is.Null);
    }

    [Test]
    public void FetchAsync_GivenConnectionBlockedByConnector_WhenCalled_ThenThrowsAccessDenied()
    {
        var sut = CreateSut(_ => throw new HttpRequestException(
            "connect failed", new BlockedAddressException("evil.example.com", IPAddress.Loopback)), out _);

        Assert.ThrowsAsync<WebPageAccessDeniedException>(() => sut.FetchAsync(PageUrl, CancellationToken.None));
    }

    [Test]
    public void FetchAsync_GivenNetworkError_WhenCalled_ThenThrowsFetchFailed()
    {
        var sut = CreateSut(_ => throw new HttpRequestException("name resolution failed"), out _);

        Assert.ThrowsAsync<WebPageFetchFailedException>(() => sut.FetchAsync(PageUrl, CancellationToken.None));
    }

    [Test]
    public void FetchAsync_GivenHandlerCancelledByTimeout_WhenCalled_ThenThrowsFetchTimeout()
    {
        var handler = new HangingHandler();
        var sut = new WebPageContentFetcher(new HttpClient(handler));
        using var cts = new CancellationTokenSource();

        // 以較短的呼叫端逾時模擬不了「內部逾時」，因此直接讓 handler 丟出與逾時相同的 TaskCanceledException。
        handler.ThrowOnSend = new TaskCanceledException("timeout");

        Assert.ThrowsAsync<WebPageFetchTimeoutException>(() => sut.FetchAsync(PageUrl, cts.Token));
    }

    [Test]
    public void FetchAsync_GivenCallerCancels_WhenCalled_ThenPropagatesCancellationInsteadOfTimeout()
    {
        var sut = CreateSut(_ => Html("x"), out _);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAsync<TaskCanceledException>(() => sut.FetchAsync(PageUrl, cts.Token));
    }

    private sealed class HangingHandler : HttpMessageHandler
    {
        public Exception? ThrowOnSend { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => ThrowOnSend is null ? Task.FromCanceled<HttpResponseMessage>(cancellationToken) : Task.FromException<HttpResponseMessage>(ThrowOnSend);
    }

    private sealed class CountingStream(Stream inner) : Stream
    {
        public long TotalBytesRead { get; private set; }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => inner.Length;
        public override long Position { get => inner.Position; set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = inner.Read(buffer, offset, count);
            TotalBytesRead += read;
            return read;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var read = await inner.ReadAsync(buffer, cancellationToken);
            TotalBytesRead += read;
            return read;
        }
    }

    [Test]
    public void FetchAsync_GivenEmptyAppShell_WhenCalled_ThenThrowsRequiresJavaScript()
    {
        var sut = CreateSut(_ => Html("<html><head><title>App</title></head><body><div id=\"root\"></div><script src=\"/a.js\"></script></body></html>"), out _);

        Assert.ThrowsAsync<WebPageRequiresJavaScriptException>(() => sut.FetchAsync(PageUrl, CancellationToken.None));
    }

    [Test]
    public async Task FetchAsync_GivenAppShellWithStructuredData_WhenCalled_ThenReturnsFallbackText()
    {
        const string body = "這是從 JSON-LD 取得的文章內容，長度足夠通過最小字數限制，不需要執行 JavaScript。";
        var html = $$$"""<html><head><title>App</title><script type="application/ld+json">{"articleBody":"{{{body}}}"}</script></head><body><div id="root"></div></body></html>""";
        var sut = CreateSut(_ => Html(html), out _);

        var result = await sut.FetchAsync(PageUrl, CancellationToken.None);

        Assert.That(result.Text, Does.Contain(body));
    }

    [Test]
    public async Task FetchAsync_GivenShortOrdinaryPage_WhenCalled_ThenDoesNotThrowRequiresJavaScript()
    {
        var sut = CreateSut(_ => Html("<body><p>tiny</p></body>"), out _);

        var result = await sut.FetchAsync(PageUrl, CancellationToken.None);

        Assert.That(result.Text, Is.EqualTo("tiny"));
    }
}

file static class ResponseExtensions
{
    public static HttpResponseMessage Also(this HttpResponseMessage response, Action<HttpResponseMessage> action)
    {
        action(response);
        return response;
    }
}

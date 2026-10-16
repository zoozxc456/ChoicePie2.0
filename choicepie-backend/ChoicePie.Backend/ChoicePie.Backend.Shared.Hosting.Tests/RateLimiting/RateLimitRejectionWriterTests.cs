using System.Text.Json;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using ChoicePie.Backend.Shared.Hosting.RateLimiting;
using Microsoft.AspNetCore.Http;

namespace ChoicePie.Backend.Shared.Hosting.Tests.RateLimiting;

[TestFixture]
public class RateLimitRejectionWriterTests
{
    [Test]
    public async Task WriteAsync_GivenRetryAfter_WhenCalled_ThenWrites429EnvelopeWithRetryAfterHeader()
    {
        var body = new MemoryStream();
        var context = new DefaultHttpContext { Response = { Body = body } };

        await RateLimitRejectionWriter.WriteAsync(context, TimeSpan.FromSeconds(42), CancellationToken.None);

        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status429TooManyRequests));
        Assert.That(context.Response.Headers.RetryAfter.ToString(), Is.EqualTo("42"));
        body.Position = 0;
        var response = JsonSerializer.Deserialize<ApiErrorResponse>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Assert.That(response.Status, Is.False);
        Assert.That(response.Code, Is.EqualTo("TooManyRequests"));
        Assert.That(response.Message, Is.EqualTo("請求過於頻繁，請稍後再試"));
    }

    [Test]
    public async Task WriteAsync_GivenNoRetryAfterMetadata_WhenCalled_ThenOmitsRetryAfterHeader()
    {
        var context = new DefaultHttpContext { Response = { Body = new MemoryStream() } };

        await RateLimitRejectionWriter.WriteAsync(context, null, CancellationToken.None);

        Assert.That(context.Response.Headers.ContainsKey("Retry-After"), Is.False);
    }
}

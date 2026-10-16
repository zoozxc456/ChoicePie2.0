using ChoicePie.Backend.Shared.Hosting.API.Response;
using Microsoft.AspNetCore.Http;

namespace ChoicePie.Backend.Shared.Hosting.RateLimiting;

/// <summary>
/// 速率限制被拒絕時，以統一的 ApiResponse 錯誤 envelope 回傳 429 並附上 Retry-After。
/// </summary>
public static class RateLimitRejectionWriter
{
    public const string Message = "請求過於頻繁，請稍後再試";

    public static async Task WriteAsync(HttpContext context, TimeSpan? retryAfter, CancellationToken cancellationToken)
    {
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (retryAfter is { } delay)
        {
            context.Response.Headers.RetryAfter = ((int)Math.Ceiling(delay.TotalSeconds)).ToString();
        }

        var response = ApiErrorResponse.CreateErrorResponse(
            nameof(System.Net.HttpStatusCode.TooManyRequests), Message, context.TraceIdentifier);

        await context.Response.WriteAsJsonAsync(response, cancellationToken);
    }
}

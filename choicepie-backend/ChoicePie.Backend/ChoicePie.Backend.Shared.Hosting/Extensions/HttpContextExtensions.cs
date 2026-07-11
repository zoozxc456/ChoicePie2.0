using Microsoft.AspNetCore.Http;

namespace ChoicePie.Backend.Shared.Hosting.Extensions;

public static class HttpContextExtensions
{
    extension(HttpContext context)
    {
        public string? GetClientIpAddress()
            => context.Connection.RemoteIpAddress?.ToString()
               ?? context.Request.Headers["X-Real-IP"].FirstOrDefault()
               ?? context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
    }
}

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ChoicePie.Backend.Shared.Hosting.Extensions;

public static class AntiforgeryMiddlewareExtensions
{
    // Writes the request token (not the cookie token) into XSRF-TOKEN so the
    // frontend can read it and echo it back in the x-csrf-token header.
    public static IApplicationBuilder UseAntiforgeryTokenCookie(
        this IApplicationBuilder app,
        Action<CookieOptions>? configureCookie = null)
    {
        app.Use((context, next) =>
        {
            var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
            var tokens = antiforgery.GetAndStoreTokens(context);

            var cookieOptions = new CookieOptions { HttpOnly = false };
            configureCookie?.Invoke(cookieOptions);

            context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, cookieOptions);

            return next();
        });

        return app;
    }
}

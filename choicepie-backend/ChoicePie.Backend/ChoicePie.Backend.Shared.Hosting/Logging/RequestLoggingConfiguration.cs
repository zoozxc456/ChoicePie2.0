using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.AspNetCore;
using Serilog.Events;

namespace ChoicePie.Backend.Shared.Hosting.Logging;

public static class RequestLoggingConfiguration
{
    // 與 HttpContextCurrentUserService 相同，直接用 JWT 的 "sub" 字面值，避免為了常數引入 JWT 套件。
    private const string SubjectClaimType = "sub";

    extension(IApplicationBuilder app)
    {
        /// <summary>
        /// 每個 request 結束時記一行摘要。要放在 <c>UseForwardedHeaders</c> 之後（ClientIp 才是真實 IP）、
        /// <c>UseExceptionHandler</c> 之前（例外已被轉成 500，不會在這裡再被記一次）。
        /// </summary>
        public IApplicationBuilder UseChoicePieRequestLogging()
            => app.UseSerilogRequestLogging(options =>
            {
                Configure(options);
                // 未指定時 middleware 會寫進靜態的 Log.Logger——那是 bootstrap logger（preserveStaticLogger），
                // 沒有 enrichment 也沒有 Sentry。
                options.Logger = app.ApplicationServices.GetRequiredService<ILogger>();
            });
    }

    public static void Configure(RequestLoggingOptions options)
    {
        options.GetLevel = GetLevel;
        options.EnrichDiagnosticContext = EnrichDiagnosticContext;
    }

    public static LogEventLevel GetLevel(HttpContext httpContext, double elapsedMilliseconds, Exception? exception)
    {
        // 探針每幾秒打一次，記下來只會淹沒 Loki。
        if (httpContext.Request.Path.StartsWithSegments("/health"))
        {
            return LogEventLevel.Verbose;
        }

        // 5xx 的例外已由 DefaultExceptionHandler 以 LogError 記錄並送 Sentry；這裡若也記 Error，
        // Sentry 會多開一個不帶例外的 issue（ShouldReport(null) 會放行）。
        return exception is not null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError
            ? LogEventLevel.Warning
            : LogEventLevel.Information;
    }

    /// <summary>只記 id 與 IP，不記 email、名稱、token 或 body。</summary>
    public static void EnrichDiagnosticContext(IDiagnosticContext diagnosticContext, HttpContext httpContext)
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString());

        // RequestPath 帶著實際 id，聚合查詢要用 route pattern（api/v{version:apiVersion}/quizzes/{id}）。
        if (httpContext.GetEndpoint() is RouteEndpoint { RoutePattern.RawText: { } routePattern })
        {
            diagnosticContext.Set("RoutePattern", routePattern);
        }

        if (httpContext.User.FindFirst(SubjectClaimType)?.Value is { } userId)
        {
            diagnosticContext.Set("UserId", userId);
        }
    }
}

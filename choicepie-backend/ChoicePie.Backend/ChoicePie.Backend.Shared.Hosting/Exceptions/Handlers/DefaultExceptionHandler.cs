using ChoicePie.Backend.Shared.Hosting.API.Response;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChoicePie.Backend.Shared.Hosting.Exceptions.Handlers;

public class DefaultExceptionHandler(
    ILogger<DefaultExceptionHandler> logger,
    IHostEnvironment env) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception has occurred. Request path: {Path}",
            httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var message = PresentationMessage(exception);

        var response = ResponseHelper.InternalServerError(httpContext.TraceIdentifier, message);

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }

    private string PresentationMessage(Exception ex) =>
        env.IsDevelopment()
            ? ex.ToString()
            : "伺服器發生未預期的錯誤，請稍後再試。";
}
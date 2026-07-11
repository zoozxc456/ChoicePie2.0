using ChoicePie.Backend.Shared.Hosting.API.Response;
using ChoicePie.Backend.Shared.Kernel.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ChoicePie.Backend.Shared.Hosting.Exceptions.Handlers;

public class DomainExceptionHandler(ILogger<DomainExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DomainException domainEx) return false;

        logger.Log(
            domainEx.LogLevel,
            exception,
            "Domain Error: {InternalMessage} | Code: {ErrorCode}",
            domainEx.Message,
            domainEx.ErrorCode
        );

        httpContext.Response.StatusCode = (int)domainEx.StatusCode;
        var response = new ApiErrorResponse
        {
            Code = domainEx.ErrorCode,
            Status = false,
            Message = domainEx.UserFriendlyMessage,
            TraceId = httpContext.TraceIdentifier
        };

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
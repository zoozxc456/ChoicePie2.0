using System.ComponentModel.DataAnnotations;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ChoicePie.Backend.Shared.Hosting.Exceptions.Handlers;

public class BadRequestExceptionHandler(ILogger<BadRequestExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var response = exception switch
        {
            ValidationException vex => ResponseHelper.BadRequest(
                message: "一個或多個驗證錯誤發生。",
                traceId: httpContext.TraceIdentifier,
                errors: GetValidationErrors(vex)),

            InvalidOperationException => ResponseHelper.BadRequest(
                message: exception.Message,
                traceId: httpContext.TraceIdentifier),

            _ => null
        };

        if (response == null) return false;

        logger.LogWarning("Bad request exception: {Message}", exception.Message);

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }

    private static Dictionary<string, string[]> GetValidationErrors(ValidationException ex)
    {
        var errorMessage = ex.ValidationResult.ErrorMessage ?? "驗證失敗";

        if (ex.ValidationResult.MemberNames.Any())
        {
            return ex.ValidationResult.MemberNames
                .ToDictionary(memberName => memberName, _ => new[] { errorMessage });
        }

        return new Dictionary<string, string[]>
        {
            { string.Empty, [errorMessage] }
        };
    }
}
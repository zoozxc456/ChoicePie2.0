using System.Net;

namespace ChoicePie.Backend.Shared.Hosting.API.Response;

public static class ResponseHelper
{
    /// <summary>
    /// 回傳成功的泛型資料
    /// </summary>
    public static ApiResponse<T> Success<T>(T data, string message = "Success") => new()
    {
        Code = nameof(HttpStatusCode.OK),
        Status = true,
        Data = data,
        Message = message
    };

    /// <summary>
    /// 僅回傳成功訊息 (無資料)
    /// </summary>
    public static ApiResponse Success(string message = "Success") => new()
    {
        Code = nameof(HttpStatusCode.OK),
        Status = true,
        Data = null,
        Message = message
    };

    public static ApiErrorResponse BadRequest(string message, string traceId,
        Dictionary<string, string[]>? errors = null)
        => ApiErrorResponse.CreateErrorResponse(nameof(HttpStatusCode.BadRequest), message, traceId, errors);

    public static ApiErrorResponse UnAuthorized(string traceId, string message = "Unauthorized")
        => ApiErrorResponse.CreateErrorResponse(nameof(HttpStatusCode.Unauthorized), message, traceId);

    public static ApiErrorResponse Forbidden(string traceId, string message = "Forbidden")
        => ApiErrorResponse.CreateErrorResponse(nameof(HttpStatusCode.Forbidden), message, traceId);

    public static ApiErrorResponse NotFound(string traceId, string message = "Resource not found")
        => ApiErrorResponse.CreateErrorResponse(nameof(HttpStatusCode.NotFound), message, traceId);

    public static ApiErrorResponse Conflict(string traceId, string message = "Resource conflict")
        => ApiErrorResponse.CreateErrorResponse(nameof(HttpStatusCode.Conflict), message, traceId);

    public static ApiErrorResponse InternalServerError(string traceId, string message = "Internal server error")
        => ApiErrorResponse.CreateErrorResponse(nameof(HttpStatusCode.InternalServerError), message, traceId);
}
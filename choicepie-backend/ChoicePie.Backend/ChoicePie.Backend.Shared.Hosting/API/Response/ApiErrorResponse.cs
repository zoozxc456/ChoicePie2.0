namespace ChoicePie.Backend.Shared.Hosting.API.Response;

/// <summary>
/// 統一的錯誤回應結構
/// </summary>
public record ApiErrorResponse : ApiResponse
{
    public string? TraceId { get; init; }

    public Dictionary<string, string[]>? Errors { get; init; }

    public static ApiErrorResponse CreateErrorResponse(
        string code,
        string message,
        string? traceId = null,
        Dictionary<string, string[]>? errors = null) => new()
    {
        Code = code,
        Status = false,
        Message = message,
        Data = null,
        Errors = errors,
        TraceId = traceId
    };
}
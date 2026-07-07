namespace ChoicePie.Backend.Shared.Hosting.API.Response;

public record ApiResponse<T>
{
    public required string Code { get; init; }
    public required bool Status { get; init; }
    public T? Data { get; init; }
    public required string Message { get; init; }

    public static ApiResponse<T> Create(string code, bool status, T? data, string message) =>
        new()
        {
            Code = code,
            Status = status,
            Data = data,
            Message = message
        };
}

public record ApiResponse : ApiResponse<object>;
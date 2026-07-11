using System.ComponentModel.DataAnnotations;

namespace ChoicePie.Backend.Shared.Application.Exceptions;

public sealed class AggregateValidationException(IReadOnlyDictionary<string, string[]> errors)
    : ValidationException("一個或多個驗證錯誤發生。")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}

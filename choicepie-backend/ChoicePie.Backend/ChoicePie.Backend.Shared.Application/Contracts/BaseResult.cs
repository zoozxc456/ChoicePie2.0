namespace ChoicePie.Backend.Shared.Application.Contracts;

public record BaseResult<T>(IEnumerable<T> Items);
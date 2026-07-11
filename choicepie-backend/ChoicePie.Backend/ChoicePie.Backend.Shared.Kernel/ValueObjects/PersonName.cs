using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Shared.Kernel.ValueObjects;

public sealed record PersonName : ValueObject
{
    public const int MinLength = 2;
    public const int MaxLength = 20;

    public string Value { get; }

    private PersonName(string value)
    {
        Value = value;
    }

    public static PersonName Create(string value, Func<string, Exception> invalidNameExceptionFactory)
    {
        var trimmedLength = value.Trim().Length;

        if (trimmedLength < MinLength || trimmedLength > MaxLength)
        {
            throw invalidNameExceptionFactory(value);
        }

        return new PersonName(value);
    }
}

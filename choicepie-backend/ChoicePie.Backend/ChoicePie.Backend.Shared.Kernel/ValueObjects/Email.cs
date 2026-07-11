using System.Text.RegularExpressions;
using ChoicePie.Backend.Shared.Kernel.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Shared.Kernel.ValueObjects;

public sealed partial record Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();

        if (!EmailFormatRegex().IsMatch(normalized))
        {
            throw new InvalidEmailException(value);
        }

        return new Email(normalized);
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailFormatRegex();
}

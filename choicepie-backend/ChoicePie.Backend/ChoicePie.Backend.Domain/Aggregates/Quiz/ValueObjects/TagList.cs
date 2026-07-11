using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.ValueObjects;

public sealed record TagList : ValueObject
{
    public IReadOnlyList<string> Values { get; }

    private TagList(IReadOnlyList<string> values)
    {
        Values = values;
    }

    public static TagList Create(IEnumerable<string> tags)
    {
        return new TagList(tags.Distinct(StringComparer.OrdinalIgnoreCase).ToList());
    }
}

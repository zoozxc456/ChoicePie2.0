using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;

public sealed record Difficulty(int Id, string Name) : Enumeration<Difficulty>(Id, Name)
{
    public static readonly Difficulty Beginner = new(1, "beginner");
    public static readonly Difficulty Intermediate = new(2, "intermediate");
    public static readonly Difficulty Expert = new(3, "expert");
}

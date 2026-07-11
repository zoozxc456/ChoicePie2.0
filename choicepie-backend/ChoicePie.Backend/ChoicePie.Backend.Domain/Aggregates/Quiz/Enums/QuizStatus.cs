using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;

public sealed record QuizStatus(int Id, string Name) : Enumeration<QuizStatus>(Id, Name)
{
    public static readonly QuizStatus Draft = new(1, "draft");
    public static readonly QuizStatus Published = new(2, "published");
    public static readonly QuizStatus Archived = new(3, "archived");
}

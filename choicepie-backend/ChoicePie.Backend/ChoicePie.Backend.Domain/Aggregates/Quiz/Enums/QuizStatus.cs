using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;

public sealed record QuizStatus(int Id, string Name) : Enumeration<QuizStatus>(Id, Name)
{
    public static readonly QuizStatus Draft = new(1, nameof(Draft));
    public static readonly QuizStatus Published = new(2, nameof(Published));
    public static readonly QuizStatus Archived = new(3, nameof(Archived));
    public static readonly QuizStatus Deleted = new(4, nameof(Deleted));
    public static readonly QuizStatus TakenDown = new(5, nameof(TakenDown));
}
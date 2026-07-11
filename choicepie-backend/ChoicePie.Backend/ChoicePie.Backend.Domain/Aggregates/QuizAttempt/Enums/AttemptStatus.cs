using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Enums;

public sealed record AttemptStatus(int Id, string Name) : Enumeration<AttemptStatus>(Id, Name)
{
    public static readonly AttemptStatus InProgress = new(1, "in_progress");
    public static readonly AttemptStatus Completed = new(2, "completed");
}

using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.QuizAttempt.ValueObjects;

public sealed record AttemptScore : ValueObject
{
    private const decimal PassThreshold = 60m;

    public decimal Value { get; }

    public bool Passed => Value >= PassThreshold;

    private AttemptScore(decimal value)
    {
        Value = value;
    }

    public static AttemptScore FromCorrectCount(int correctCount, int totalCount)
    {
        var value = correctCount / (decimal)totalCount * 100m;

        return new AttemptScore(value);
    }
}

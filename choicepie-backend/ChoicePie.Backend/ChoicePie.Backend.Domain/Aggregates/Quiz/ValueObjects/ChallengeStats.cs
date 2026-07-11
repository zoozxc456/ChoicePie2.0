using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.ValueObjects;

public sealed record ChallengeStats : ValueObject
{
    public static readonly ChallengeStats None = new(0, 0m);

    public int Count { get; }
    public decimal PassRate { get; }

    private ChallengeStats(int count, decimal passRate)
    {
        Count = count;
        PassRate = passRate;
    }

    public ChallengeStats RecordOutcome(bool passed)
    {
        var newCount = Count + 1;
        var previousPassedTotal = PassRate * Count;
        var newPassRate = (previousPassedTotal + (passed ? 100m : 0m)) / newCount;

        return new ChallengeStats(newCount, newPassRate);
    }
}

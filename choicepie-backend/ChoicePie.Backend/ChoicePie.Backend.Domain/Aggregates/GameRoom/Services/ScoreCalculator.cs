namespace ChoicePie.Backend.Domain.Aggregates.GameRoom.Services;

public static class ScoreCalculator
{
    public static int Calculate(TimeSpan elapsed, TimeSpan timeLimit, bool isCorrect)
    {
        if (!isCorrect || elapsed < TimeSpan.Zero || elapsed >= timeLimit) return 0;

        return elapsed.TotalSeconds switch
        {
            <= 3 => 1000,
            <= 6 => 800,
            <= 10 => 600,
            _ => 400
        };
    }
}

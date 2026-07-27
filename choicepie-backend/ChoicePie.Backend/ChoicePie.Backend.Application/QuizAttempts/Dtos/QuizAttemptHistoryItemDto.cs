namespace ChoicePie.Backend.Application.QuizAttempts.Dtos;

public sealed record QuizAttemptHistoryItemDto(
    Guid Id,
    decimal Score,
    bool Passed,
    DateTime StartedAt,
    DateTime CompletedAt,
    long DurationMs);

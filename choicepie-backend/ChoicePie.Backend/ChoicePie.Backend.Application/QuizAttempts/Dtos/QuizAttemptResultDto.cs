namespace ChoicePie.Backend.Application.QuizAttempts.Dtos;

public sealed record QuizAttemptResultDto(
    Guid Id,
    Guid QuizId,
    string QuizTitle,
    Guid MemberId,
    decimal? Score,
    bool? Passed,
    DateTime StartedAt,
    DateTime? CompletedAt,
    IReadOnlyList<QuizAttemptAnswerResultDto> Answers);

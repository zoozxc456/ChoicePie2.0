namespace ChoicePie.Backend.Application.QuizAttempts.Dtos;

public sealed record QuizAttemptAnswerResultDto(
    Guid QuestionId,
    string QuestionText,
    int? SelectedOptionIndex,
    int? CorrectOptionIndex,
    bool IsCorrect,
    string? Explanation);

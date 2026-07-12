namespace ChoicePie.Backend.Application.GameSessions.Dtos;

public sealed record GameSessionWrongAnswerDto(
    string QuestionText,
    IReadOnlyList<string> Options,
    int MyAnswerIndex,
    int CorrectAnswerIndex,
    string Explanation);

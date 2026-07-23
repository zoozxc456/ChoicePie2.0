namespace ChoicePie.Backend.Application.GameSessions.Dtos;

public sealed record GameSessionQuestionBreakdownDto(
    string QuestionText,
    IReadOnlyList<GameSessionOptionStatDto> Options,
    int CorrectCount,
    int AnsweredCount);

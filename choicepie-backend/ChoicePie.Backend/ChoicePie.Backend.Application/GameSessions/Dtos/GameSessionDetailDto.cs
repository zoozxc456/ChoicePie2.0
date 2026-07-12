namespace ChoicePie.Backend.Application.GameSessions.Dtos;

public sealed record GameSessionDetailDto(
    Guid Id,
    string RoomCode,
    Guid QuizId,
    string QuizTitle,
    string CoverEmoji,
    string CoverGradient,
    DateTime PlayedAtUtc,
    int PlayerCount,
    int QuestionCount,
    bool IsHost,
    IReadOnlyList<GameSessionRankEntryDto> Rankings,
    int? MyRank,
    int? MyScore,
    IReadOnlyList<GameSessionWrongAnswerDto> MyWrongAnswers);

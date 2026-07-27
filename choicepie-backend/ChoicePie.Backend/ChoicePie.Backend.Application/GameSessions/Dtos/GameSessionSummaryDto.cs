namespace ChoicePie.Backend.Application.GameSessions.Dtos;

public sealed record GameSessionSummaryDto(
    Guid Id,
    string RoomCode,
    Guid QuizId,
    string QuizTitle,
    string CoverEmoji,
    string CoverGradient,
    DateTime PlayedAtUtc,
    int PlayerCount,
    int QuestionCount,
    string TopPlayerName,
    int TopPlayerScore,
    int? MyRank,
    int? MyScore);

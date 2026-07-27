using ChoicePie.Backend.Application.Quizzes.Dtos;

namespace ChoicePie.Backend.Application.AdminQuizzes.Dtos;

public sealed record AdminQuizDetailDto(
    Guid Id,
    string Title,
    string? Description,
    string CoverEmoji,
    string CoverGradient,
    string Difficulty,
    string Status,
    int ChallengeCount,
    decimal PassRate,
    Guid CreatorId,
    string CreatorName,
    string? CreatorAvatar,
    IReadOnlyList<QuestionDto> Questions,
    int QuestionCount,
    IReadOnlyList<string> Tags,
    int ShareCount,
    int FavoriteCount,
    string? TakedownReason,
    Guid? TakedownBy,
    DateTime? TakedownAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);

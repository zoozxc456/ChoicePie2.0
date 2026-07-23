namespace ChoicePie.Backend.Application.CreatorFollows.Dtos;

public sealed record CreatorProfileDto(
    Guid Id,
    string Name,
    string? Avatar,
    int QuizCount,
    int ChallengeCount,
    bool IsFollowing);

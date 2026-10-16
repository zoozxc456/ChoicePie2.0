namespace ChoicePie.Backend.Application.CreatorFollows.Dtos;

public sealed record FollowedCreatorDto(
    Guid Id,
    string Name,
    string? Avatar,
    int QuizCount,
    DateTime FollowedAt);

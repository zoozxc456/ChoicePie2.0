namespace ChoicePie.Backend.Application.Comments.Dtos;

public sealed record CommentDto(
    Guid Id,
    Guid QuizId,
    Guid UserId,
    string UserName,
    string? UserAvatar,
    string Text,
    DateTime CreatedAt);

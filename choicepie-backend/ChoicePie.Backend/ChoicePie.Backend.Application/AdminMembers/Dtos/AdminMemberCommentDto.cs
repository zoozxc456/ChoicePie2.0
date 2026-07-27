namespace ChoicePie.Backend.Application.AdminMembers.Dtos;

public sealed record AdminMemberCommentDto(
    Guid Id,
    Guid QuizId,
    string QuizTitle,
    string Text,
    DateTime CreatedAt);

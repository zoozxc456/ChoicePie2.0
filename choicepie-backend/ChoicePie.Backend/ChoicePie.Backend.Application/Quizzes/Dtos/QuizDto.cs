using ChoicePie.Backend.Domain.Aggregates.Quiz;

namespace ChoicePie.Backend.Application.Quizzes.Dtos;

public sealed record QuizDto(
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
    IReadOnlyList<string> Tags,
    int ShareCount,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public int QuestionCount => Questions.Count;

    public static QuizDto FromDomain(Quiz quiz, string creatorName, string? creatorAvatar) => new(
        quiz.Id,
        quiz.Title,
        quiz.Description,
        quiz.Cover.Emoji,
        quiz.Cover.Gradient,
        quiz.Difficulty.Name,
        quiz.Status.Name.ToLowerInvariant(),
        quiz.ChallengeCount,
        quiz.PassRate,
        quiz.OwnerId,
        creatorName,
        creatorAvatar,
        quiz.Questions.Select(QuestionDto.FromDomain).ToList(),
        quiz.Tags,
        quiz.ShareCount,
        quiz.CreatedAt,
        quiz.LastModifiedAt);
}

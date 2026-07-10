using ChoicePie.Backend.Domain.Aggregates.Quiz;

namespace ChoicePie.Backend.Application.Quizzes.Dtos;

public sealed record QuizSummaryDto(
    Guid Id,
    string Title,
    string? Description,
    string CoverEmoji,
    string CoverGradient,
    string Difficulty,
    int QuestionCount,
    int ChallengeCount,
    decimal PassRate,
    Guid CreatorId,
    string CreatorName,
    string? CreatorAvatar,
    IReadOnlyList<string> Tags,
    bool IsPublic,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static QuizSummaryDto FromDomain(Quiz quiz, string creatorName, string? creatorAvatar) => new(
        quiz.Id,
        quiz.Title,
        quiz.Description,
        quiz.CoverEmoji,
        quiz.CoverGradient,
        quiz.Difficulty.Name,
        quiz.QuestionCount,
        quiz.ChallengeCount,
        quiz.PassRate,
        quiz.OwnerId,
        creatorName,
        creatorAvatar,
        quiz.Tags,
        quiz.IsPublic,
        quiz.CreatedAt,
        quiz.LastModifiedAt);
}

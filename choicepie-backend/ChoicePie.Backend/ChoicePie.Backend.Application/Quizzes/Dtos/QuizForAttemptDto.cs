using System.ComponentModel;
using ChoicePie.Backend.Domain.Aggregates.Quiz;

namespace ChoicePie.Backend.Application.Quizzes.Dtos;

// 會被快取，L1 命中時多個請求共用同一個實例；只能用 with 產生修改後的副本，不可就地修改。
[ImmutableObject(true)]
public sealed record QuizForAttemptDto(
    Guid Id,
    string Title,
    string? Description,
    string? CoverImageUrl,
    string CoverEmoji,
    string CoverGradient,
    string Difficulty,
    Guid CreatorId,
    string CreatorName,
    string? CreatorAvatar,
    IReadOnlyList<QuestionForAttemptDto> Questions,
    IReadOnlyList<string> Tags)
{
    public static QuizForAttemptDto FromDomain(Quiz quiz, string creatorName, string? creatorAvatar) => new(
        quiz.Id,
        quiz.Title,
        quiz.Description,
        quiz.Cover.ImageUrl,
        quiz.Cover.Emoji,
        quiz.Cover.Gradient,
        quiz.Difficulty.Name,
        quiz.OwnerId,
        creatorName,
        creatorAvatar,
        quiz.Questions.Select(QuestionForAttemptDto.FromDomain).ToList(),
        quiz.Tags);
}

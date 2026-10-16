using System.ComponentModel;
using ChoicePie.Backend.Domain.Aggregates.Quiz;

namespace ChoicePie.Backend.Application.Quizzes.Dtos;

// 會被快取，L1 命中時多個請求共用同一個實例；只能用 with 產生修改後的副本，不可就地修改。
[ImmutableObject(true)]
public sealed record QuizDto(
    Guid Id,
    string Title,
    string? Description,
    string? CoverImageUrl,
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
    IReadOnlyList<QuestionStubDto> QuestionStubs,
    int QuestionCount,
    IReadOnlyList<string> Tags,
    int ShareCount,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    // includeQuestions=false is for viewers who aren't the quiz owner - they can see basic info
    // and ratings but not question content, so Questions comes back empty while QuestionStubs
    // (id only) still lets the client select questions for a game room.
    public static QuizDto FromDomain(Quiz quiz, string creatorName, string? creatorAvatar, bool includeQuestions = true) => new(
        quiz.Id,
        quiz.Title,
        quiz.Description,
        quiz.Cover.ImageUrl,
        quiz.Cover.Emoji,
        quiz.Cover.Gradient,
        quiz.Difficulty.Name,
        quiz.Status.Name.ToLowerInvariant(),
        quiz.ChallengeCount,
        quiz.PassRate,
        quiz.OwnerId,
        creatorName,
        creatorAvatar,
        includeQuestions ? quiz.Questions.Select(QuestionDto.FromDomain).ToList() : [],
        quiz.Questions.Select(QuestionStubDto.FromDomain).ToList(),
        quiz.QuestionCount,
        quiz.Tags,
        quiz.ShareCount,
        quiz.CreatedAt,
        quiz.LastModifiedAt);
}

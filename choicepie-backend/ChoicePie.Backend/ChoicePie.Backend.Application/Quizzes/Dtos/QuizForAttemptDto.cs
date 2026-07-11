using ChoicePie.Backend.Domain.Aggregates.Quiz;

namespace ChoicePie.Backend.Application.Quizzes.Dtos;

public sealed record QuizForAttemptDto(
    Guid Id,
    string Title,
    string? Description,
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
        quiz.CoverEmoji,
        quiz.CoverGradient,
        quiz.Difficulty.Name,
        quiz.OwnerId,
        creatorName,
        creatorAvatar,
        quiz.Questions.Select(QuestionForAttemptDto.FromDomain).ToList(),
        quiz.Tags);
}

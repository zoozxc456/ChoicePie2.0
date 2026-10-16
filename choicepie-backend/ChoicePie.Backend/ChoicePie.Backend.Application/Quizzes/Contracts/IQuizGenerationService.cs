using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;

namespace ChoicePie.Backend.Application.Quizzes.Contracts;

public interface IQuizGenerationService
{
    Task<GeneratedQuestionsResult> GenerateAsync(
        string content, int questionCount, Difficulty difficulty, CancellationToken cancellationToken,
        WebPageSource? source = null);
}

public sealed record GeneratedQuestionsResult(IReadOnlyList<GeneratedQuestion> Questions, int TokensUsed);

public sealed record GeneratedQuestion(string Text, IReadOnlyList<string> Options, int AnswerIndex, string Explanation);

/// <summary>內容來自網頁時的來源資訊，供提示詞標示「以下為網頁文字」。</summary>
public sealed record WebPageSource(Uri Url, string? Title);

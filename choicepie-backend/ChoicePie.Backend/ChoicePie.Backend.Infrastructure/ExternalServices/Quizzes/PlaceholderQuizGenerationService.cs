using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;

namespace ChoicePie.Backend.Infrastructure.ExternalServices.Quizzes;

/// <summary>
/// 暫時性佔位實作：尚未串接真正的 LLM 供應商，先回傳假資料讓出題流程可被端到端執行。
/// </summary>
public sealed class PlaceholderQuizGenerationService : IQuizGenerationService, IScopedDependency
{
    public Task<GeneratedQuestionsResult> GenerateAsync(
        string content, int questionCount, Difficulty difficulty, CancellationToken cancellationToken)
    {
        var questions = Enumerable.Range(1, questionCount)
            .Select(i => new GeneratedQuestion(
                $"Placeholder question {i}",
                ["A", "B", "C", "D"],
                0,
                "Placeholder explanation"))
            .ToList();

        return Task.FromResult(new GeneratedQuestionsResult(questions, TokensUsed: 0));
    }
}

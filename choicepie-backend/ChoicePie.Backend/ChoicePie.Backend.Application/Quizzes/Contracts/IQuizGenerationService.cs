using ChoicePie.Backend.Domain.Aggregates.Quiz;

namespace ChoicePie.Backend.Application.Quizzes.Contracts;

public interface IQuizGenerationService
{
    Task<GeneratedQuestionsResult> GenerateAsync(
        string content, int questionCount, Difficulty difficulty, CancellationToken cancellationToken);
}

public sealed record GeneratedQuestionsResult(IReadOnlyList<GeneratedQuestion> Questions, int TokensUsed);

public sealed record GeneratedQuestion(string Text, IReadOnlyList<string> Options, int AnswerIndex, string Explanation);

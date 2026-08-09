using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;

namespace ChoicePie.Backend.Infrastructure.ExternalServices.Quizzes.Providers;

public interface IQuizGenerationProvider
{
    string ProviderName { get; }

    Task<GeneratedQuestionsResult> GenerateAsync(
        string content, int questionCount, Difficulty difficulty, CancellationToken cancellationToken);
}

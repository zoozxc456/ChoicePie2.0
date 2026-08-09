using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Infrastructure.ExternalServices.Quizzes.Providers;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChoicePie.Backend.Infrastructure.ExternalServices.Quizzes;

public sealed class QuizGenerationService : IQuizGenerationService, IScopedDependency
{
    private static readonly TimeSpan CallTimeout = TimeSpan.FromSeconds(30);

    private readonly IReadOnlyDictionary<string, IQuizGenerationProvider> providersByName;
    private readonly AiQuizGenerationSettings settings;
    private readonly ILogger<QuizGenerationService> logger;

    public QuizGenerationService(
        IEnumerable<IQuizGenerationProvider> providers,
        IOptions<AiQuizGenerationSettings> settings,
        ILogger<QuizGenerationService> logger)
    {
        providersByName = providers.ToDictionary(p => p.ProviderName, StringComparer.OrdinalIgnoreCase);
        this.settings = settings.Value;
        this.logger = logger;
    }

    public async Task<GeneratedQuestionsResult> GenerateAsync(
        string content, int questionCount, Difficulty difficulty, CancellationToken cancellationToken)
    {
        if (!providersByName.TryGetValue(settings.Provider, out var provider))
        {
            throw new QuizGenerationFailedException($"Unknown AI quiz generation provider configured: {settings.Provider}");
        }

        try
        {
            return await CallWithTimeoutAsync(provider, content, questionCount, difficulty, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "AI quiz generation attempt 1 failed via {Provider}, retrying once", provider.ProviderName);
        }

        try
        {
            return await CallWithTimeoutAsync(provider, content, questionCount, difficulty, cancellationToken);
        }
        catch (QuizGenerationFailedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new QuizGenerationFailedException($"Provider {provider.ProviderName} failed after retry.", ex);
        }
    }

    private static async Task<GeneratedQuestionsResult> CallWithTimeoutAsync(
        IQuizGenerationProvider provider, string content, int questionCount, Difficulty difficulty,
        CancellationToken cancellationToken)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(CallTimeout);

        try
        {
            return await provider.GenerateAsync(content, questionCount, difficulty, timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new QuizGenerationFailedException($"Provider {provider.ProviderName} timed out after {CallTimeout.TotalSeconds}s.");
        }
    }
}

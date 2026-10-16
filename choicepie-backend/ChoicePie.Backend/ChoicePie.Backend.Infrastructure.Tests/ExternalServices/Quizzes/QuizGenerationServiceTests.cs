using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Infrastructure.ExternalServices.Quizzes;
using ChoicePie.Backend.Infrastructure.ExternalServices.Quizzes.Providers;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace ChoicePie.Backend.Infrastructure.Tests.ExternalServices.Quizzes;

[TestFixture]
public class QuizGenerationServiceTests
{
    private IQuizGenerationProvider _provider = null!;
    private ILogger<QuizGenerationService> _logger = null!;
    private QuizGenerationService _sut = null!;

    private static readonly GeneratedQuestionsResult SampleResult =
        new([new GeneratedQuestion("Q1?", ["A", "B", "C", "D"], 0, "because")], 42);

    [SetUp]
    public void SetUp()
    {
        _provider = Substitute.For<IQuizGenerationProvider>();
        _provider.ProviderName.Returns("Anthropic");
        _logger = Substitute.For<ILogger<QuizGenerationService>>();

        var settings = Options.Create(new AiQuizGenerationSettings
        {
            Provider = "Anthropic", ApiKey = "test-key", Model = "claude-sonnet-5"
        });

        _sut = new QuizGenerationService([_provider], settings, _logger);
    }

    [Test]
    public async Task GenerateAsync_GivenProviderSucceedsOnFirstAttempt_WhenCalled_ThenReturnsResultAndDoesNotRetry()
    {
        _provider.GenerateAsync("content", 5, Difficulty.Beginner, Arg.Any<CancellationToken>())
            .Returns(SampleResult);

        var result = await _sut.GenerateAsync("content", 5, Difficulty.Beginner, CancellationToken.None);

        Assert.That(result, Is.EqualTo(SampleResult));
        await _provider.Received(1).GenerateAsync("content", 5, Difficulty.Beginner, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GenerateAsync_GivenProviderFailsOnceThenSucceeds_WhenCalled_ThenRetriesAndReturnsResult()
    {
        _provider.GenerateAsync("content", 5, Difficulty.Beginner, Arg.Any<CancellationToken>())
            .Returns(
                _ => throw new InvalidOperationException("transient failure"),
                _ => SampleResult);

        var result = await _sut.GenerateAsync("content", 5, Difficulty.Beginner, CancellationToken.None);

        Assert.That(result, Is.EqualTo(SampleResult));
        await _provider.Received(2).GenerateAsync("content", 5, Difficulty.Beginner, Arg.Any<CancellationToken>());
    }

    [Test]
    public void GenerateAsync_GivenProviderFailsTwice_WhenCalled_ThenThrowsQuizGenerationFailedExceptionAfterOneRetry()
    {
        _provider.GenerateAsync("content", 5, Difficulty.Beginner, Arg.Any<CancellationToken>())
            .Returns<GeneratedQuestionsResult>(_ => throw new InvalidOperationException("permanent failure"));

        Assert.ThrowsAsync<QuizGenerationFailedException>(
            () => _sut.GenerateAsync("content", 5, Difficulty.Beginner, CancellationToken.None));
    }

    [Test]
    public async Task GenerateAsync_GivenProviderFailsTwice_WhenCalled_ThenCallsProviderExactlyTwice()
    {
        _provider.GenerateAsync("content", 5, Difficulty.Beginner, Arg.Any<CancellationToken>())
            .Returns<GeneratedQuestionsResult>(_ => throw new InvalidOperationException("permanent failure"));

        try
        {
            await _sut.GenerateAsync("content", 5, Difficulty.Beginner, CancellationToken.None);
        }
        catch (QuizGenerationFailedException)
        {
            // expected; asserting call count below
        }

        await _provider.Received(2).GenerateAsync("content", 5, Difficulty.Beginner, Arg.Any<CancellationToken>());
    }

    [Test]
    public void GenerateAsync_GivenUnconfiguredProvider_WhenCalled_ThenThrowsQuizGenerationFailedException()
    {
        var settings = Options.Create(new AiQuizGenerationSettings
        {
            Provider = "OpenAI", ApiKey = "test-key", Model = "gpt-5"
        });
        var sut = new QuizGenerationService([_provider], settings, _logger);

        Assert.ThrowsAsync<QuizGenerationFailedException>(
            () => sut.GenerateAsync("content", 5, Difficulty.Beginner, CancellationToken.None));
    }
}

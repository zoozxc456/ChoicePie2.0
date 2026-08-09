using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Infrastructure.ExternalServices.Quizzes.Providers;
using ChoicePie.Backend.Infrastructure.Tests.TestHelpers;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using Microsoft.Extensions.Options;

namespace ChoicePie.Backend.Infrastructure.Tests.ExternalServices.Quizzes.Providers;

[TestFixture]
public class AnthropicQuizGenerationProviderTests
{
    private static readonly IOptions<AiQuizGenerationSettings> Settings = Options.Create(new AiQuizGenerationSettings
    {
        Provider = "Anthropic", ApiKey = "test-key", Model = "claude-sonnet-5"
    });

    private static AnthropicQuizGenerationProvider CreateSut(
        Func<HttpRequestMessage, HttpResponseMessage> respond, out FakeHttpMessageHandler handler)
    {
        handler = new FakeHttpMessageHandler(respond);
        var httpClient = new HttpClient(handler);
        return new AnthropicQuizGenerationProvider(httpClient, Settings);
    }

    private static HttpResponseMessage SuccessResponse(int questionCount, int inputTokens = 100, int outputTokens = 50)
    {
        var questions = Enumerable.Range(1, questionCount)
            .Select(i => new
            {
                text = $"Question {i}?", options = new[] { "A", "B", "C", "D" }, answerIndex = 0, explanation = "because"
            })
            .ToArray();

        var body = new
        {
            content = new object[]
            {
                new
                {
                    type = "tool_use", name = "submit_quiz_questions", input = new { questions }
                }
            },
            usage = new { input_tokens = inputTokens, output_tokens = outputTokens }
        };

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(body)
        };
    }

    [Test]
    public async Task GenerateAsync_GivenSuccessfulToolUseResponse_WhenCalled_ThenReturnsMappedQuestionsAndTokenUsage()
    {
        var sut = CreateSut(_ => SuccessResponse(questionCount: 2, inputTokens: 100, outputTokens: 50), out _);

        var result = await sut.GenerateAsync("some content", 2, Difficulty.Beginner, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Questions, Has.Count.EqualTo(2));
            Assert.That(result.Questions[0].Text, Is.EqualTo("Question 1?"));
            Assert.That(result.Questions[0].Options, Is.EqualTo(new[] { "A", "B", "C", "D" }));
            Assert.That(result.TokensUsed, Is.EqualTo(150));
        });
    }

    [Test]
    public async Task GenerateAsync_GivenRequest_WhenCalled_ThenSendsToolChoiceForcingSubmitQuizQuestionsTool()
    {
        var sut = CreateSut(_ => SuccessResponse(questionCount: 3), out var handler);

        await sut.GenerateAsync("some content", 3, Difficulty.Expert, CancellationToken.None);

        Assert.That(handler.LastRequestBody, Is.Not.Null);
        using var payload = JsonDocument.Parse(handler.LastRequestBody!);
        var root = payload.RootElement;

        Assert.Multiple(() =>
        {
            Assert.That(root.GetProperty("model").GetString(), Is.EqualTo("claude-sonnet-5"));
            Assert.That(root.GetProperty("tool_choice").GetProperty("name").GetString(), Is.EqualTo("submit_quiz_questions"));
            Assert.That(root.GetProperty("tools")[0].GetProperty("name").GetString(), Is.EqualTo("submit_quiz_questions"));
            Assert.That(root.GetProperty("messages")[0].GetProperty("content").GetString(), Does.Contain("some content"));
        });
    }

    [Test]
    public void GenerateAsync_GivenNonSuccessHttpStatus_WhenCalled_ThenThrowsQuizGenerationFailedException()
    {
        var sut = CreateSut(_ => new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent("rate limited")
        }, out _);

        Assert.ThrowsAsync<QuizGenerationFailedException>(
            () => sut.GenerateAsync("content", 3, Difficulty.Beginner, CancellationToken.None));
    }

    [Test]
    public void GenerateAsync_GivenResponseWithoutToolUseBlock_WhenCalled_ThenThrowsQuizGenerationFailedException()
    {
        var sut = CreateSut(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                content = new object[] { new { type = "text", text = "sorry, I can't help with that" } },
                usage = new { input_tokens = 10, output_tokens = 5 }
            })
        }, out _);

        Assert.ThrowsAsync<QuizGenerationFailedException>(
            () => sut.GenerateAsync("content", 3, Difficulty.Beginner, CancellationToken.None));
    }

    [Test]
    public void GenerateAsync_GivenFewerQuestionsThanRequested_WhenCalled_ThenThrowsQuizGenerationFailedException()
    {
        var sut = CreateSut(_ => SuccessResponse(questionCount: 2), out _);

        Assert.ThrowsAsync<QuizGenerationFailedException>(
            () => sut.GenerateAsync("content", 5, Difficulty.Beginner, CancellationToken.None));
    }
}

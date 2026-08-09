using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using Microsoft.Extensions.Options;

namespace ChoicePie.Backend.Infrastructure.ExternalServices.Quizzes.Providers;

public sealed class AnthropicQuizGenerationProvider : IQuizGenerationProvider, IScopedDependency
{
    private const string ToolName = "submit_quiz_questions";
    private const string ApiVersion = "2023-06-01";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient httpClient;
    private readonly AiQuizGenerationSettings settings;

    public AnthropicQuizGenerationProvider(HttpClient httpClient, IOptions<AiQuizGenerationSettings> settings)
    {
        this.httpClient = httpClient;
        this.settings = settings.Value;

        this.httpClient.BaseAddress = new Uri("https://api.anthropic.com/v1/");
        this.httpClient.DefaultRequestHeaders.Add("x-api-key", this.settings.ApiKey);
        this.httpClient.DefaultRequestHeaders.Add("anthropic-version", ApiVersion);
    }

    public string ProviderName => "Anthropic";

    public async Task<GeneratedQuestionsResult> GenerateAsync(
        string content, int questionCount, Difficulty difficulty, CancellationToken cancellationToken)
    {
        var request = BuildRequest(content, questionCount, difficulty);

        using var httpResponse = await httpClient.PostAsJsonAsync("messages", request, JsonOptions, cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            var errorBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new QuizGenerationFailedException($"Anthropic API returned {(int)httpResponse.StatusCode}: {errorBody}");
        }

        var response = await httpResponse.Content.ReadFromJsonAsync<AnthropicMessageResponse>(JsonOptions, cancellationToken)
                        ?? throw new QuizGenerationFailedException("Anthropic API returned an empty response body.");

        var toolUseBlock = response.Content.FirstOrDefault(block => block.Type == "tool_use" && block.Name == ToolName)
                            ?? throw new QuizGenerationFailedException("Anthropic response did not contain the expected tool_use block.");

        var toolInput = toolUseBlock.Input.Deserialize<QuizQuestionsToolInput>(JsonOptions)
                         ?? throw new QuizGenerationFailedException("Anthropic tool_use input could not be parsed.");

        if (toolInput.Questions.Count != questionCount)
        {
            throw new QuizGenerationFailedException(
                $"Anthropic returned {toolInput.Questions.Count} questions, expected {questionCount}.");
        }

        var questions = toolInput.Questions
            .Select(q => new GeneratedQuestion(q.Text, q.Options, q.AnswerIndex, q.Explanation))
            .ToList();

        var tokensUsed = response.Usage.InputTokens + response.Usage.OutputTokens;

        return new GeneratedQuestionsResult(questions, tokensUsed);
    }

    private AnthropicMessageRequest BuildRequest(string content, int questionCount, Difficulty difficulty)
    {
        var difficultyGuidance = difficulty.Name switch
        {
            "beginner" => "題目應直接對應內容中明確提到的事實，答案容易在原文中找到。",
            "intermediate" => "題目可包含需要理解上下文或連結多個段落資訊才能作答的內容。",
            "expert" => "題目應要求推論、比較或應用內容中的概念，不能只是原文的字面重述。",
            _ => "題目難度應與內容的重點平衡。"
        };

        var systemPrompt = $"""
            你是一位專業的出題老師，負責根據使用者提供的內容出選擇題。

            規則：
            1. 僅根據提供的內容出題，禁止虛構內容中沒有的資訊。
            2. 全部使用繁體中文（台灣用語），不可使用簡體字或中國大陸用語。
            3. 每題必須恰好有 4 個選項，且僅有 1 個正確答案。
            4. 錯誤選項（幹擾項）必須具有合理性與鑑別度，不可一眼看穿，也不可有多個選項同樣正確。
            5. 每題需附上簡短的解釋，說明為什麼正確答案是對的。
            6. {difficultyGuidance}
            7. 必須使用 {ToolName} 工具回傳結果，不要用純文字回答。
            """;

        var userMessage = $"請根據以下內容出 {questionCount} 題選擇題：\n\n{content}";

        return new AnthropicMessageRequest(
            Model: settings.Model,
            MaxTokens: 4096,
            System: systemPrompt,
            Messages: [new AnthropicMessage("user", userMessage)],
            Tools: [BuildToolDefinition()],
            ToolChoice: new AnthropicToolChoice("tool", ToolName));
    }

    private static AnthropicToolDefinition BuildToolDefinition()
    {
        var schema = JsonSerializer.SerializeToElement(new
        {
            type = "object",
            properties = new
            {
                questions = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            text = new { type = "string" },
                            options = new { type = "array", items = new { type = "string" }, minItems = 4, maxItems = 4 },
                            answerIndex = new { type = "integer", minimum = 0, maximum = 3 },
                            explanation = new { type = "string" }
                        },
                        required = new[] { "text", "options", "answerIndex", "explanation" }
                    }
                }
            },
            required = new[] { "questions" }
        });

        return new AnthropicToolDefinition(ToolName, "提交產生好的選擇題清單。", schema);
    }

    private sealed record AnthropicMessageRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("max_tokens")] int MaxTokens,
        [property: JsonPropertyName("system")] string System,
        [property: JsonPropertyName("messages")] IReadOnlyList<AnthropicMessage> Messages,
        [property: JsonPropertyName("tools")] IReadOnlyList<AnthropicToolDefinition> Tools,
        [property: JsonPropertyName("tool_choice")] AnthropicToolChoice ToolChoice);

    private sealed record AnthropicMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record AnthropicToolDefinition(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("input_schema")] JsonElement InputSchema);

    private sealed record AnthropicToolChoice(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("name")] string Name);

    private sealed record AnthropicMessageResponse(
        [property: JsonPropertyName("content")] IReadOnlyList<AnthropicContentBlock> Content,
        [property: JsonPropertyName("usage")] AnthropicUsage Usage);

    private sealed record AnthropicContentBlock(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("input")] JsonElement Input);

    private sealed record AnthropicUsage(
        [property: JsonPropertyName("input_tokens")] int InputTokens,
        [property: JsonPropertyName("output_tokens")] int OutputTokens);

    private sealed record QuizQuestionsToolInput(
        [property: JsonPropertyName("questions")] IReadOnlyList<QuizQuestionToolItem> Questions);

    private sealed record QuizQuestionToolItem(
        [property: JsonPropertyName("text")] string Text,
        [property: JsonPropertyName("options")] IReadOnlyList<string> Options,
        [property: JsonPropertyName("answerIndex")] int AnswerIndex,
        [property: JsonPropertyName("explanation")] string Explanation);
}

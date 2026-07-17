using System.Net;
using System.Net.Http.Json;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Hosting.API.Response;

namespace ChoicePie.Backend.WebApi.Tests;

public sealed class QuizzesControllerTests
{
    private CustomWebApplicationFactory _factory = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new CustomWebApplicationFactory();
        await _factory.InitializeAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown() => await _factory.DisposeAsync();

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var (client, _, _) = await GameHubTestClient.CreateAuthenticatedHostClientAsync(_factory);
        return client;
    }

    private static object OneQuestionPayload(string text = "1 + 1 = ?", int answerIndex = 1) => new
    {
        Text = text,
        Options = new[] { "1", "2", "3", "4" },
        AnswerIndex = answerIndex,
        Explanation = "basic math"
    };

    private static object CreateQuizBody(string title = "Integration Test Quiz") => new
    {
        Title = title,
        Description = "for QuizzesController tests",
        CoverEmoji = "🎯",
        CoverGradient = "from-red-500 to-orange-500",
        Difficulty = "beginner",
        Tags = new[] { "test" },
        Questions = new[] { OneQuestionPayload() }
    };

    private static async Task<QuizDto> CreateQuizAsync(HttpClient client, string title = "Integration Test Quiz")
    {
        var response = await client.PostAsJsonAsync("/api/v1/quizzes", CreateQuizBody(title));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<QuizDto>>();
        return body!.Data!;
    }

    [Test]
    public async Task CreateAsync_GivenValidRequest_WhenCalled_ThenReturnsDraftQuizWithQuestion()
    {
        using var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/quizzes", CreateQuizBody());

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<QuizDto>>();
        Assert.That(body!.Data!.Status, Is.EqualTo("draft"));
        Assert.That(body.Data.QuestionCount, Is.EqualTo(1));
    }

    [Test]
    public async Task CreateAsync_GivenNoAuthentication_WhenCalled_ThenReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/quizzes", CreateQuizBody());

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task GetByIdAsync_GivenExistingQuiz_WhenCalled_ThenReturnsQuiz()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var quiz = await CreateQuizAsync(client);

        var response = await client.GetAsync($"/api/v1/quizzes/{quiz.Id}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<QuizDto>>();
        Assert.That(body!.Data!.Id, Is.EqualTo(quiz.Id));
    }

    [Test]
    public async Task GetByIdAsync_GivenNonExistentQuiz_WhenCalled_ThenReturnsNotFound()
    {
        using var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync($"/api/v1/quizzes/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.That(body!.Code, Is.EqualTo("QUIZ_NOT_FOUND"));
    }

    [Test]
    public async Task UpdateAsync_GivenOwner_WhenCalled_ThenUpdatesTitleAndTags()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var quiz = await CreateQuizAsync(client);

        var response = await client.PutAsJsonAsync($"/api/v1/quizzes/{quiz.Id}",
            new { Title = "Updated Title", Description = "updated", Tags = new[] { "updated-tag" } });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<QuizDto>>();
        Assert.That(body!.Data!.Title, Is.EqualTo("Updated Title"));
        Assert.That(body.Data.Tags, Is.EqualTo(new[] { "updated-tag" }));
    }

    [Test]
    public async Task UpdateAsync_GivenNonOwner_WhenCalled_ThenReturnsForbidden()
    {
        using var ownerClient = await CreateAuthenticatedClientAsync();
        var quiz = await CreateQuizAsync(ownerClient);
        using var otherClient = await CreateAuthenticatedClientAsync();

        var response = await otherClient.PutAsJsonAsync($"/api/v1/quizzes/{quiz.Id}",
            new { Title = "Hijacked", Description = (string?)null, Tags = Array.Empty<string>() });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.That(body!.Code, Is.EqualTo("QUIZ_FORBIDDEN"));
    }

    [Test]
    public async Task AddQuestionAsync_ThenUpdateQuestionAsync_ThenRemoveQuestionAsync_GivenDraftQuiz_WhenCalledInSequence_ThenQuestionCountTracksCorrectly()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var quiz = await CreateQuizAsync(client);

        var addResponse = await client.PostAsJsonAsync($"/api/v1/quizzes/{quiz.Id}/questions", OneQuestionPayload("2 + 2 = ?", 3));
        Assert.That(addResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var afterAdd = await addResponse.Content.ReadFromJsonAsync<ApiResponse<QuizDto>>();
        Assert.That(afterAdd!.Data!.QuestionCount, Is.EqualTo(2));
        var secondQuestionId = afterAdd.Data.Questions[1].Id;

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/quizzes/{quiz.Id}/questions/{secondQuestionId}",
            OneQuestionPayload("2 + 2 = ? (edited)", 3));
        Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var afterUpdate = await updateResponse.Content.ReadFromJsonAsync<ApiResponse<QuizDto>>();
        Assert.That(afterUpdate!.Data!.Questions[1].Text, Is.EqualTo("2 + 2 = ? (edited)"));

        var removeResponse = await client.DeleteAsync($"/api/v1/quizzes/{quiz.Id}/questions/{secondQuestionId}");
        Assert.That(removeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var afterRemove = await removeResponse.Content.ReadFromJsonAsync<ApiResponse<QuizDto>>();
        Assert.That(afterRemove!.Data!.QuestionCount, Is.EqualTo(1));
    }

    [Test]
    public async Task PublishAsync_GivenDraftWithQuestions_WhenCalled_ThenStatusBecomesPublished()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var quiz = await CreateQuizAsync(client);

        var response = await client.PostAsync($"/api/v1/quizzes/{quiz.Id}/publish", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<QuizDto>>();
        Assert.That(body!.Data!.Status, Is.EqualTo("published"));
    }

    [Test]
    public async Task PublishAsync_GivenDraftWithNoQuestions_WhenCalled_ThenReturnsBadRequest()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var quiz = await CreateQuizAsync(client);
        var removeResponse = await client.DeleteAsync($"/api/v1/quizzes/{quiz.Id}/questions/{quiz.Questions[0].Id}");
        removeResponse.EnsureSuccessStatusCode();

        var response = await client.PostAsync($"/api/v1/quizzes/{quiz.Id}/publish", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.That(body!.Code, Is.EqualTo("INVALID_QUIZ"));
    }

    [Test]
    public async Task PublishAsync_GivenAlreadyPublishedQuiz_WhenCalledAgain_ThenReturnsBadRequest()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var quiz = await CreateQuizAsync(client);
        await client.PostAsync($"/api/v1/quizzes/{quiz.Id}/publish", null);

        var response = await client.PostAsync($"/api/v1/quizzes/{quiz.Id}/publish", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task UnpublishAsync_GivenPublishedQuiz_WhenCalled_ThenStatusBecomesDraft()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var quiz = await CreateQuizAsync(client);
        await client.PostAsync($"/api/v1/quizzes/{quiz.Id}/publish", null);

        var response = await client.PostAsync($"/api/v1/quizzes/{quiz.Id}/unpublish", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<QuizDto>>();
        Assert.That(body!.Data!.Status, Is.EqualTo("draft"));
    }

    [Test]
    public async Task ArchiveAsync_GivenDraftQuiz_WhenCalled_ThenStatusBecomesArchived()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var quiz = await CreateQuizAsync(client);

        var response = await client.PostAsync($"/api/v1/quizzes/{quiz.Id}/archive", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<QuizDto>>();
        Assert.That(body!.Data!.Status, Is.EqualTo("archived"));
    }

    [Test]
    public async Task DeleteAsync_GivenOwner_WhenCalled_ThenQuizNoLongerRetrievable()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var quiz = await CreateQuizAsync(client);

        var deleteResponse = await client.DeleteAsync($"/api/v1/quizzes/{quiz.Id}");
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var getResponse = await client.GetAsync($"/api/v1/quizzes/{quiz.Id}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GenerateQuestionsAsync_GivenValidContent_WhenCalled_ThenReturnsPlaceholderQuestions()
    {
        using var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/quizzes/generate-questions", new
        {
            Content = new string('x', 40),
            QuestionCount = 3,
            Difficulty = "beginner"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<GenerateQuestionsResultDto>>();
        Assert.That(body!.Data!.Questions, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task GetPreviewAsync_GivenPublishedQuiz_WhenCalledAnonymously_ThenReturnsQuizForAttempt()
    {
        using var ownerClient = await CreateAuthenticatedClientAsync();
        var quiz = await CreateQuizAsync(ownerClient);
        await ownerClient.PostAsync($"/api/v1/quizzes/{quiz.Id}/publish", null);

        using var anonymousClient = _factory.CreateClient();
        var response = await anonymousClient.GetAsync($"/api/v1/quizzes/{quiz.Id}/preview");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<QuizForAttemptDto>>();
        Assert.That(body!.Data!.Id, Is.EqualTo(quiz.Id));
    }

    [Test]
    public async Task ListAsync_GivenMineTrue_WhenCalled_ThenReturnsOnlyOwnQuizzes()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var uniqueTitle = $"Mine-{Guid.NewGuid()}";
        await CreateQuizAsync(client, uniqueTitle);

        var response = await client.GetAsync("/api/v1/quizzes?mine=true&pageSize=100");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<QuizSummaryDto>>>();
        Assert.That(body!.Data!.Items.Any(q => q.Title == uniqueTitle), Is.True);
    }

    [Test]
    public async Task GetTagsAsync_GivenQuizWithTag_WhenCalled_ThenIncludesThatTag()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var uniqueTag = $"tag-{Guid.NewGuid()}";
        var response = await client.PostAsJsonAsync("/api/v1/quizzes", new
        {
            Title = "Tagged Quiz",
            Description = (string?)null,
            CoverEmoji = "🎯",
            CoverGradient = "from-red-500 to-orange-500",
            Difficulty = "beginner",
            Tags = new[] { uniqueTag },
            Questions = new[] { OneQuestionPayload() }
        });
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<ApiResponse<QuizDto>>();

        // GetTagsAsync 只統計已發布題庫的標籤（見 QuizQueryService.GetTagsAsync），草稿不算。
        var publishResponse = await client.PostAsync($"/api/v1/quizzes/{created!.Data!.Id}/publish", null);
        publishResponse.EnsureSuccessStatusCode();

        var tagsResponse = await client.GetAsync("/api/v1/quizzes/tags");

        Assert.That(tagsResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await tagsResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<string>>>();
        Assert.That(body!.Data, Does.Contain(uniqueTag));
    }
}

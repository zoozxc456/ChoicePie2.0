using System.Net;
using System.Net.Http.Json;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Application.QuizAttempts.Dtos;
using ChoicePie.Backend.Shared.Hosting.API.Response;

namespace ChoicePie.Backend.WebApi.Tests;

public sealed class QuizAttemptsControllerTests
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

    private static async Task<QuizDto> CreateAndPublishTwoQuestionQuizAsync(HttpClient ownerClient)
    {
        var createResponse = await ownerClient.PostAsJsonAsync("/api/v1/quizzes", new
        {
            Title = "Attempt Test Quiz",
            Description = "for QuizAttempts tests",
            CoverEmoji = "🎯",
            CoverGradient = "from-red-500 to-orange-500",
            Difficulty = "beginner",
            Tags = new[] { "test" },
            Questions = new[]
            {
                new { Text = "1 + 1 = ?", Options = new[] { "1", "2", "3", "4" }, AnswerIndex = 1, Explanation = "e1" },
                new { Text = "2 + 2 = ?", Options = new[] { "1", "2", "3", "4" }, AnswerIndex = 3, Explanation = "e2" }
            }
        });
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<QuizDto>>();
        var quiz = created!.Data!;

        var publishResponse = await ownerClient.PostAsync($"/api/v1/quizzes/{quiz.Id}/publish", null);
        publishResponse.EnsureSuccessStatusCode();

        return quiz;
    }

    // EF 沒有保證 Quiz.Questions 的回傳順序，不能假設 Questions[0] 對應建立時的第一題，
    // 用題目文字對回正確答案索引才穩定。
    private static int CorrectAnswerIndexFor(string questionText) => questionText switch
    {
        "1 + 1 = ?" => 1,
        "2 + 2 = ?" => 3,
        _ => throw new ArgumentOutOfRangeException(nameof(questionText))
    };

    private async Task<(HttpClient Client, StartAttemptResultDto Attempt)> StartAttemptAsync(Guid quizId)
    {
        var client = await CreateAuthenticatedClientAsync();
        var response = await client.PostAsJsonAsync("/api/v1/quiz-attempts", new { QuizId = quizId });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<StartAttemptResultDto>>();
        return (client, body!.Data!);
    }

    [Test]
    public async Task StartAsync_GivenPublishedQuiz_WhenCalled_ThenReturnsAttemptWithQuestionsButNoAnswers()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var quiz = await CreateAndPublishTwoQuestionQuizAsync(ownerClient);

        var (_, attempt) = await StartAttemptAsync(quiz.Id);

        Assert.That(attempt.AttemptId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(attempt.Quiz.Questions, Has.Count.EqualTo(2));
        var body = System.Text.Json.JsonSerializer.Serialize(attempt);
        Assert.That(body.ToLowerInvariant(), Does.Not.Contain("answerindex"));
    }

    [Test]
    public async Task StartAsync_GivenDraftQuiz_WhenCalled_ThenReturnsBadRequest()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var createResponse = await ownerClient.PostAsJsonAsync("/api/v1/quizzes", new
        {
            Title = "Draft Quiz",
            Description = (string?)null,
            CoverEmoji = "🎯",
            CoverGradient = "from-red-500 to-orange-500",
            Difficulty = "beginner",
            Tags = new[] { "test" },
            Questions = new[]
            {
                new { Text = "Q?", Options = new[] { "1", "2", "3", "4" }, AnswerIndex = 0, Explanation = "e" }
            }
        });
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<QuizDto>>();

        var attemptClient = await CreateAuthenticatedClientAsync();
        var response = await attemptClient.PostAsJsonAsync("/api/v1/quiz-attempts", new { QuizId = created!.Data!.Id });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.That(body!.Code, Is.EqualTo("QUIZ_NOT_PUBLISHED"));
    }

    [Test]
    public async Task StartAsync_GivenNoAuthentication_WhenCalled_ThenReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/quiz-attempts", new { QuizId = Guid.NewGuid() });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task SubmitAnswerAsync_GivenTwoDifferentQuestions_WhenSubmittedInTwoSeparateRequests_ThenBothAnswersPersist()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var quiz = await CreateAndPublishTwoQuestionQuizAsync(ownerClient);
        var (client, attempt) = await StartAttemptAsync(quiz.Id);

        var firstSubmit = await client.PostAsJsonAsync($"/api/v1/quiz-attempts/{attempt.AttemptId}/answers",
            new
            {
                QuestionId = attempt.Quiz.Questions[0].Id,
                SelectedOptionIndex = CorrectAnswerIndexFor(attempt.Quiz.Questions[0].Text)
            });
        Assert.That(firstSubmit.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var secondSubmit = await client.PostAsJsonAsync($"/api/v1/quiz-attempts/{attempt.AttemptId}/answers",
            new
            {
                QuestionId = attempt.Quiz.Questions[1].Id,
                SelectedOptionIndex = CorrectAnswerIndexFor(attempt.Quiz.Questions[1].Text)
            });
        Assert.That(secondSubmit.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var completeResponse = await client.PostAsync($"/api/v1/quiz-attempts/{attempt.AttemptId}/complete", null);
        Assert.That(completeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await completeResponse.Content.ReadFromJsonAsync<ApiResponse<QuizAttemptResultDto>>();

        Assert.That(result!.Data!.Answers, Has.Count.EqualTo(2));
        Assert.That(result.Data.Score, Is.EqualTo(100));
        Assert.That(result.Data.Passed, Is.True);
    }

    [Test]
    public async Task SubmitAnswerAsync_GivenSameQuestionTwice_WhenCalled_ThenLatestAnswerWins()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var quiz = await CreateAndPublishTwoQuestionQuizAsync(ownerClient);
        var (client, attempt) = await StartAttemptAsync(quiz.Id);
        var question = attempt.Quiz.Questions[0];
        var correctIndex = CorrectAnswerIndexFor(question.Text);
        var wrongIndex = correctIndex == 0 ? 1 : 0;

        await client.PostAsJsonAsync($"/api/v1/quiz-attempts/{attempt.AttemptId}/answers",
            new { QuestionId = question.Id, SelectedOptionIndex = wrongIndex });
        await client.PostAsJsonAsync($"/api/v1/quiz-attempts/{attempt.AttemptId}/answers",
            new { QuestionId = question.Id, SelectedOptionIndex = correctIndex });

        var completeResponse = await client.PostAsync($"/api/v1/quiz-attempts/{attempt.AttemptId}/complete", null);
        var result = await completeResponse.Content.ReadFromJsonAsync<ApiResponse<QuizAttemptResultDto>>();

        var answer = result!.Data!.Answers.Single(a => a.QuestionId == question.Id);
        Assert.That(answer.SelectedOptionIndex, Is.EqualTo(correctIndex));
        Assert.That(answer.IsCorrect, Is.True);
    }

    [Test]
    public async Task SubmitAnswerAsync_GivenNonOwner_WhenCalled_ThenReturnsForbidden()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var quiz = await CreateAndPublishTwoQuestionQuizAsync(ownerClient);
        var (_, attempt) = await StartAttemptAsync(quiz.Id);

        var otherClient = await CreateAuthenticatedClientAsync();
        var response = await otherClient.PostAsJsonAsync($"/api/v1/quiz-attempts/{attempt.AttemptId}/answers",
            new { QuestionId = attempt.Quiz.Questions[0].Id, SelectedOptionIndex = 0 });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.That(body!.Code, Is.EqualTo("QUIZ_ATTEMPT_FORBIDDEN"));
    }

    [Test]
    public async Task SubmitAnswerAsync_GivenUnknownQuestionId_WhenCalled_ThenReturnsBadRequest()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var quiz = await CreateAndPublishTwoQuestionQuizAsync(ownerClient);
        var (client, attempt) = await StartAttemptAsync(quiz.Id);

        var response = await client.PostAsJsonAsync($"/api/v1/quiz-attempts/{attempt.AttemptId}/answers",
            new { QuestionId = Guid.NewGuid(), SelectedOptionIndex = 0 });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.That(body!.Code, Is.EqualTo("INVALID_QUIZ_ATTEMPT"));
    }

    [Test]
    public async Task SubmitAnswerAsync_GivenAlreadyCompletedAttempt_WhenCalled_ThenReturnsBadRequest()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var quiz = await CreateAndPublishTwoQuestionQuizAsync(ownerClient);
        var (client, attempt) = await StartAttemptAsync(quiz.Id);
        await client.PostAsync($"/api/v1/quiz-attempts/{attempt.AttemptId}/complete", null);

        var response = await client.PostAsJsonAsync($"/api/v1/quiz-attempts/{attempt.AttemptId}/answers",
            new { QuestionId = attempt.Quiz.Questions[0].Id, SelectedOptionIndex = 0 });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.That(body!.Code, Is.EqualTo("INVALID_QUIZ_ATTEMPT_STATE"));
    }

    [Test]
    public async Task CompleteAsync_GivenPartiallyAnsweredAttempt_WhenCalled_ThenScoresOnlyAnsweredQuestionsAsCorrect()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var quiz = await CreateAndPublishTwoQuestionQuizAsync(ownerClient);
        var (client, attempt) = await StartAttemptAsync(quiz.Id);
        var question = attempt.Quiz.Questions[0];

        await client.PostAsJsonAsync($"/api/v1/quiz-attempts/{attempt.AttemptId}/answers",
            new { QuestionId = question.Id, SelectedOptionIndex = CorrectAnswerIndexFor(question.Text) });

        var completeResponse = await client.PostAsync($"/api/v1/quiz-attempts/{attempt.AttemptId}/complete", null);

        Assert.That(completeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await completeResponse.Content.ReadFromJsonAsync<ApiResponse<QuizAttemptResultDto>>();
        Assert.That(result!.Data!.Score, Is.EqualTo(50));
        Assert.That(result.Data.Passed, Is.False);
    }

    [Test]
    public async Task CompleteAsync_GivenAlreadyCompletedAttempt_WhenCalledAgain_ThenReturnsBadRequest()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var quiz = await CreateAndPublishTwoQuestionQuizAsync(ownerClient);
        var (client, attempt) = await StartAttemptAsync(quiz.Id);
        await client.PostAsync($"/api/v1/quiz-attempts/{attempt.AttemptId}/complete", null);

        var response = await client.PostAsync($"/api/v1/quiz-attempts/{attempt.AttemptId}/complete", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.That(body!.Code, Is.EqualTo("INVALID_QUIZ_ATTEMPT_STATE"));
    }

    [Test]
    public async Task CompleteAsync_GivenNonOwner_WhenCalled_ThenReturnsForbidden()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var quiz = await CreateAndPublishTwoQuestionQuizAsync(ownerClient);
        var (_, attempt) = await StartAttemptAsync(quiz.Id);

        var otherClient = await CreateAuthenticatedClientAsync();
        var response = await otherClient.PostAsync($"/api/v1/quiz-attempts/{attempt.AttemptId}/complete", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task GetByIdAsync_GivenOwnerAfterCompletion_WhenCalled_ThenReturnsAttemptWithCorrectAnswers()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var quiz = await CreateAndPublishTwoQuestionQuizAsync(ownerClient);
        var (client, attempt) = await StartAttemptAsync(quiz.Id);
        await client.PostAsJsonAsync($"/api/v1/quiz-attempts/{attempt.AttemptId}/answers",
            new { QuestionId = attempt.Quiz.Questions[0].Id, SelectedOptionIndex = 1 });
        await client.PostAsync($"/api/v1/quiz-attempts/{attempt.AttemptId}/complete", null);

        var response = await client.GetAsync($"/api/v1/quiz-attempts/{attempt.AttemptId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<QuizAttemptResultDto>>();
        Assert.That(body!.Data!.Id, Is.EqualTo(attempt.AttemptId));
        Assert.That(body.Data.CompletedAt, Is.Not.Null);
    }

    [Test]
    public async Task GetByIdAsync_GivenNonExistentAttempt_WhenCalled_ThenReturnsNotFound()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync($"/api/v1/quiz-attempts/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.That(body!.Code, Is.EqualTo("QUIZ_ATTEMPT_NOT_FOUND"));
    }

    [Test]
    public async Task GetByIdAsync_GivenNonOwner_WhenCalled_ThenReturnsForbidden()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var quiz = await CreateAndPublishTwoQuestionQuizAsync(ownerClient);
        var (_, attempt) = await StartAttemptAsync(quiz.Id);

        var otherClient = await CreateAuthenticatedClientAsync();
        var response = await otherClient.GetAsync($"/api/v1/quiz-attempts/{attempt.AttemptId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }
}

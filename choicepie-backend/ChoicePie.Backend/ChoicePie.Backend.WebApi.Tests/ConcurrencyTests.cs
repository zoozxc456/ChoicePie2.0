using System.Net;
using System.Net.Http.Json;
using ChoicePie.Backend.Application.Identity.Dtos;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Application.QuizAttempts.Dtos;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using ChoicePie.Backend.Shared.Kernel.Auth;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ChoicePie.Backend.WebApi.Tests;

/// <summary>
/// 同一時間對同一筆資料送出一批請求（連點、重送、很多人同時操作），用真的 Postgres 驗證：
/// 不會出現 500、不會產生重複資料、計數器不會少算、一次性 token 只能成功一次。
/// </summary>
public sealed class ConcurrencyTests
{
    private const int Parallelism = 8;

    private CustomWebApplicationFactory _factory = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new CustomWebApplicationFactory();
        await _factory.InitializeAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown() => await _factory.DisposeAsync();

    [Test]
    public async Task Register_GivenSameEmailAtOnce_ThenExactlyOneSucceedsAndOthersGetConflict()
    {
        using var client = CreateClient();
        var email = $"{Guid.NewGuid()}@example.com";

        var statuses = await SendAtOnceAsync(() => client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = email,
            Name = "Racer",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        }));

        Assert.That(statuses.Count(s => (int)s is >= 200 and < 300), Is.EqualTo(1));
        Assert.That(statuses.Where(s => (int)s >= 300), Is.All.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task Refresh_GivenSameRefreshTokenAtOnce_ThenOnlyOneRotationSucceeds()
    {
        using var client = CreateClient();
        var refreshToken = await RegisterAndLoginAsync(client);

        var statuses = await SendAtOnceAsync(() =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/refresh");
            request.Headers.Add(AuthHeaderNames.RefreshToken, refreshToken);
            return client.SendAsync(request);
        });

        Assert.That(statuses.Count(s => s == HttpStatusCode.OK), Is.EqualTo(1), "一次性 token 只能換出一組新 token");
        Assert.That(statuses.Where(s => s != HttpStatusCode.OK), Is.All.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Favorite_GivenSameQuizAtOnce_ThenAllSucceed()
    {
        var (client, quizId) = await CreateMemberWithPublishedQuizAsync();
        using var _ = client;

        var statuses = await SendAtOnceAsync(() => client.PutAsync($"/api/v1/quizzes/{quizId}/favorite", null));

        Assert.That(statuses, Is.All.EqualTo(HttpStatusCode.OK), "重複收藏要冪等，不能因唯一索引變成 500");
    }

    [Test]
    public async Task Report_GivenSameQuizAtOnce_ThenOnlyOnePendingReportIsCreated()
    {
        var (owner, quizId) = await CreateMemberWithPublishedQuizAsync();
        owner.Dispose();
        using var reporter = await CreateMemberClientAsync();

        var statuses = await SendAtOnceAsync(() =>
            reporter.PostAsJsonAsync($"/api/v1/quizzes/{quizId}/report", new { Reason = "Spam", Description = "dup" }));

        Assert.That(statuses.Count(s => s == HttpStatusCode.OK), Is.EqualTo(1));
        Assert.That(statuses.Where(s => s != HttpStatusCode.OK), Is.All.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task StartAttempt_GivenSameQuizAtOnce_ThenAllReturnTheSameInProgressAttempt()
    {
        var (owner, quizId) = await CreateMemberWithPublishedQuizAsync();
        owner.Dispose();
        using var player = await CreateMemberClientAsync();

        var responses = await Task.WhenAll(Enumerable.Range(0, Parallelism)
            .Select(_ => player.PostAsJsonAsync("/api/v1/quiz-attempts", new { QuizId = quizId })));

        Assert.That(responses.Select(r => r.StatusCode), Is.All.EqualTo(HttpStatusCode.OK));
        var attemptIds = await Task.WhenAll(responses.Select(async r =>
            (await r.Content.ReadFromJsonAsync<ApiResponse<StartAttemptResultDto>>())!.Data!.AttemptId));
        Assert.That(attemptIds.Distinct().Count(), Is.EqualTo(1), "同一份題庫同時只能有一筆進行中的作答");
    }

    [Test]
    public async Task CompleteAttempt_GivenSameAttemptAtOnce_ThenChallengeIsCountedOnce()
    {
        var (owner, quizId) = await CreateMemberWithPublishedQuizAsync();
        using var player = await CreateMemberClientAsync();
        var attempt = await StartAttemptAsync(player, quizId);

        var statuses = await SendAtOnceAsync(() =>
            player.PostAsync($"/api/v1/quiz-attempts/{attempt.AttemptId}/complete", null));

        Assert.That(statuses.Count(s => s == HttpStatusCode.OK), Is.EqualTo(1));
        Assert.That(statuses.Where(s => s != HttpStatusCode.OK), Has.None.EqualTo(HttpStatusCode.InternalServerError));
        Assert.That((await GetQuizAsync(owner, quizId)).ChallengeCount, Is.EqualTo(1), "連點繳交不能重複計算挑戰次數");
        owner.Dispose();
    }

    [Test]
    public async Task CompleteAttempt_GivenManyPlayersFinishingAtOnce_ThenNoChallengeOutcomeIsLost()
    {
        const int playerCount = 10;
        var (owner, quizId) = await CreateMemberWithPublishedQuizAsync();
        var players = new List<(HttpClient Client, StartAttemptResultDto Attempt)>();
        for (var i = 0; i < playerCount; i++)
        {
            var client = await CreateMemberClientAsync();
            var attempt = await StartAttemptAsync(client, quizId);
            // 一半的人答對（通過），一半不作答（未通過），通過率應該剛好 50%。
            if (i % 2 == 0)
            {
                var question = attempt.Quiz.Questions.Single();
                (await client.PostAsJsonAsync($"/api/v1/quiz-attempts/{attempt.AttemptId}/answers",
                    new { QuestionId = question.Id, SelectedOptionIndex = 1 })).EnsureSuccessStatusCode();
            }

            players.Add((client, attempt));
        }

        var responses = await Task.WhenAll(players.Select(p =>
            p.Client.PostAsync($"/api/v1/quiz-attempts/{p.Attempt.AttemptId}/complete", null)));

        Assert.That(responses.Select(r => r.StatusCode), Is.All.EqualTo(HttpStatusCode.OK));
        var quiz = await GetQuizAsync(owner, quizId);
        Assert.That(quiz.ChallengeCount, Is.EqualTo(playerCount));
        Assert.That(quiz.PassRate, Is.EqualTo(50m));
        players.ForEach(p => p.Client.Dispose());
        owner.Dispose();
    }

    [Test]
    public async Task Share_GivenManySharesAtOnce_ThenEveryShareIsCounted()
    {
        const int shareCount = 30;
        var (owner, quizId) = await CreateMemberWithPublishedQuizAsync();
        using var _ = owner;
        using var anonymous = CreateClient();

        var responses = await Task.WhenAll(Enumerable.Range(0, shareCount)
            .Select(_ => anonymous.PostAsync($"/api/v1/quizzes/{quizId}/share", null)));

        Assert.That(responses.Select(r => r.StatusCode), Is.All.EqualTo(HttpStatusCode.OK));
        Assert.That((await GetQuizAsync(owner, quizId)).ShareCount, Is.EqualTo(shareCount));
    }

    private static async Task<HttpStatusCode[]> SendAtOnceAsync(Func<Task<HttpResponseMessage>> send)
    {
        var responses = await Task.WhenAll(Enumerable.Range(0, Parallelism).Select(_ => send()));
        return responses.Select(r => r.StatusCode).ToArray();
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        HandleCookies = true
    });

    private async Task<HttpClient> CreateMemberClientAsync()
    {
        var (client, _, _) = await GameHubTestClient.CreateAuthenticatedHostClientAsync(_factory);
        return client;
    }

    private async Task<(HttpClient Owner, Guid QuizId)> CreateMemberWithPublishedQuizAsync()
    {
        var owner = await CreateMemberClientAsync();
        return (owner, await GameHubTestClient.CreateAndPublishQuizAsync(owner));
    }

    private static async Task<StartAttemptResultDto> StartAttemptAsync(HttpClient client, Guid quizId)
    {
        var response = await client.PostAsJsonAsync("/api/v1/quiz-attempts", new { QuizId = quizId });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ApiResponse<StartAttemptResultDto>>())!.Data!;
    }

    private static async Task<QuizDto> GetQuizAsync(HttpClient client, Guid quizId)
    {
        var response = await client.GetAsync($"/api/v1/quizzes/{quizId}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ApiResponse<QuizDto>>())!.Data!;
    }

    private static async Task<string> RegisterAndLoginAsync(HttpClient client)
    {
        var email = $"{Guid.NewGuid()}@example.com";
        (await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = email,
            Name = "Racer",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        })).EnsureSuccessStatusCode();

        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = "Password123!" });
        login.EnsureSuccessStatusCode();
        return (await login.Content.ReadFromJsonAsync<ApiResponse<LoginResultDto>>())!.Data!.RefreshToken;
    }
}

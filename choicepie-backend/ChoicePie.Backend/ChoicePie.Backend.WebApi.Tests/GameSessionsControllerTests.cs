using System.Net;
using System.Net.Http.Json;
using ChoicePie.Backend.Application.GameRooms.Dtos;
using ChoicePie.Backend.Application.GameSessions.Dtos;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using ChoicePie.Backend.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChoicePie.Backend.WebApi.Tests;

public sealed class GameSessionsControllerTests
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

    /// <summary>
    /// 完整跑一局一題的遊戲到結束，讓 GameSession 真的被記錄下來：
    /// Host 建題庫、發布、CreateRoom → 已登入的 Player 加入 → Host StartGame →
    /// SkipQuestion 兩次（第一次結束該題、進入 Reveal；因為只有一題，第二次會直接把整局結束並存檔）。
    /// </summary>
    private async Task<(HttpClient HostClient, HttpClient PlayerClient, string RoomCode)> PlayOneQuestionGameToCompletionAsync()
    {
        var (hostClient, hostCookies, _) = await GameHubTestClient.CreateAuthenticatedHostClientAsync(_factory);

        var createQuizResponse = await hostClient.PostAsJsonAsync("/api/v1/quizzes", new
        {
            Title = "GameSession Test Quiz",
            Description = "for GameSessionsController tests",
            CoverEmoji = "🎯",
            CoverGradient = "from-red-500 to-orange-500",
            Difficulty = "beginner",
            Tags = new[] { "test" },
            Questions = new[]
            {
                new { Text = "1 + 1 = ?", Options = new[] { "1", "2", "3", "4" }, AnswerIndex = 1, Explanation = "e" }
            }
        });
        createQuizResponse.EnsureSuccessStatusCode();
        var quiz = (await createQuizResponse.Content.ReadFromJsonAsync<ApiResponse<QuizDto>>())!.Data!;
        (await hostClient.PostAsync($"/api/v1/quizzes/{quiz.Id}/publish", null)).EnsureSuccessStatusCode();

        await using var hostConnection = await GameHubTestClient.CreateHostHubConnectionAsync(_factory, hostCookies);
        var roomCreatedTcs = new TaskCompletionSource<string>();
        hostConnection.On<string>("RoomCreated", roomCode => roomCreatedTcs.TrySetResult(roomCode));
        await hostConnection.InvokeAsync("CreateRoom", new CreateRoomRequest(quiz.Id, null, null));
        var roomCode = await roomCreatedTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        var (playerClient, playerCookies, _) = await GameHubTestClient.CreateAuthenticatedHostClientAsync(_factory);
        await using var playerConnection = await GameHubTestClient.CreateHostHubConnectionAsync(_factory, playerCookies);
        var roomStateSyncTcs = new TaskCompletionSource<RoomStateSyncDto>();
        playerConnection.On<RoomStateSyncDto>("RoomStateSync", state => roomStateSyncTcs.TrySetResult(state));
        await playerConnection.InvokeAsync("JoinRoom", roomCode, "Alice");
        await roomStateSyncTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        var questionStartTcs = new TaskCompletionSource<QuestionPayloadDto>();
        playerConnection.On<QuestionPayloadDto>("QuestionStart", q => questionStartTcs.TrySetResult(q));
        await hostConnection.InvokeAsync("StartGame", roomCode);
        await questionStartTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        var questionEndTcs = new TaskCompletionSource<bool>();
        hostConnection.On<QuestionEndPayloadDto>("QuestionEnd", _ => questionEndTcs.TrySetResult(true));
        await hostConnection.InvokeAsync("SkipQuestion", roomCode);
        await questionEndTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        var gameEndTcs = new TaskCompletionSource<bool>();
        hostConnection.On<IReadOnlyList<RankEntryDto>>("GameEnd", _ => gameEndTcs.TrySetResult(true));
        await hostConnection.InvokeAsync("SkipQuestion", roomCode);
        await gameEndTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        return (hostClient, playerClient, roomCode);
    }

    [Test]
    public async Task ListHostedAsync_GivenCompletedGame_WhenCalledByHost_ThenIncludesThatSession()
    {
        var (hostClient, _, roomCode) = await PlayOneQuestionGameToCompletionAsync();

        var response = await hostClient.GetAsync("/api/v1/game-sessions/hosted?page=1&pageSize=50");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<GameSessionSummaryDto>>>();
        Assert.That(body!.Data!.Items.Any(s => s.RoomCode == roomCode), Is.True);
    }

    [Test]
    public async Task ListPlayedAsync_GivenCompletedGame_WhenCalledByAuthenticatedPlayer_ThenIncludesThatSession()
    {
        var (_, playerClient, roomCode) = await PlayOneQuestionGameToCompletionAsync();

        var response = await playerClient.GetAsync("/api/v1/game-sessions/played?page=1&pageSize=50");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<GameSessionSummaryDto>>>();
        Assert.That(body!.Data!.Items.Any(s => s.RoomCode == roomCode), Is.True);
    }

    [Test]
    public async Task ListPlayedAsync_GivenCompletedGame_WhenCalledByHostWhoDidNotPlay_ThenDoesNotIncludeThatSession()
    {
        var (hostClient, _, roomCode) = await PlayOneQuestionGameToCompletionAsync();

        var response = await hostClient.GetAsync("/api/v1/game-sessions/played?page=1&pageSize=50");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<GameSessionSummaryDto>>>();
        Assert.That(body!.Data!.Items.Any(s => s.RoomCode == roomCode), Is.False);
    }

    [Test]
    public async Task GetByIdAsync_GivenHost_WhenCalled_ThenReturnsSessionWithIsHostTrueAndFullRankings()
    {
        var (hostClient, _, roomCode) = await PlayOneQuestionGameToCompletionAsync();
        var sessionId = await GetSessionIdByRoomCodeAsync(hostClient, roomCode);

        var response = await hostClient.GetAsync($"/api/v1/game-sessions/{sessionId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<GameSessionDetailDto>>();
        Assert.That(body!.Data!.IsHost, Is.True);
        Assert.That(body.Data.Rankings, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task GetByIdAsync_GivenPlayer_WhenCalled_ThenReturnsSessionWithMyRankPopulated()
    {
        var (hostClient, playerClient, roomCode) = await PlayOneQuestionGameToCompletionAsync();
        var sessionId = await GetSessionIdByRoomCodeAsync(hostClient, roomCode);

        var response = await playerClient.GetAsync($"/api/v1/game-sessions/{sessionId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<GameSessionDetailDto>>();
        Assert.That(body!.Data!.IsHost, Is.False);
        Assert.That(body.Data.MyRank, Is.Not.Null);
    }

    [Test]
    public async Task GetByIdAsync_GivenUnrelatedMember_WhenCalled_ThenReturnsForbidden()
    {
        var (hostClient, _, roomCode) = await PlayOneQuestionGameToCompletionAsync();
        var sessionId = await GetSessionIdByRoomCodeAsync(hostClient, roomCode);
        var unrelatedClient = await CreateAuthenticatedClientAsync();

        var response = await unrelatedClient.GetAsync($"/api/v1/game-sessions/{sessionId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.That(body!.Code, Is.EqualTo("GAME_SESSION_FORBIDDEN"));
    }

    [Test]
    public async Task GetByIdAsync_GivenNonExistentSession_WhenCalled_ThenReturnsNotFound()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync($"/api/v1/game-sessions/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.That(body!.Code, Is.EqualTo("GAME_SESSION_NOT_FOUND"));
    }

    [Test]
    public async Task ListHostedAsync_GivenNoAuthentication_WhenCalled_ThenReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/game-sessions/hosted");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    private static async Task<Guid> GetSessionIdByRoomCodeAsync(HttpClient hostClient, string roomCode)
    {
        var response = await hostClient.GetAsync("/api/v1/game-sessions/hosted?page=1&pageSize=50");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<GameSessionSummaryDto>>>();
        return body!.Data!.Items.Single(s => s.RoomCode == roomCode).Id;
    }
}

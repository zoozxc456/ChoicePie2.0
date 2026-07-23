using System.Net.Http.Json;
using ChoicePie.Backend.Application.GameRooms.Dtos;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using ChoicePie.Backend.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChoicePie.Backend.WebApi.Tests;

public sealed class GameHubTests
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

    private static async Task<Guid> CreateAndPublishQuizAsync(HttpClient hostClient)
    {
        var createResponse = await hostClient.PostAsJsonAsync("/api/v1/quizzes", new
        {
            Title = "Integration Test Quiz",
            Description = "for GameHub tests",
            CoverEmoji = "🎯",
            CoverGradient = "from-red-500 to-orange-500",
            Difficulty = "beginner",
            Tags = new[] { "test" },
            Questions = new[]
            {
                new
                {
                    Text = "1 + 1 = ?",
                    Options = new[] { "1", "2", "3", "4" },
                    AnswerIndex = 1,
                    Explanation = "basic math"
                }
            }
        });
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<QuizDto>>();
        var quizId = created!.Data!.Id;

        var publishResponse = await hostClient.PostAsync($"/api/v1/quizzes/{quizId}/publish", null);
        publishResponse.EnsureSuccessStatusCode();

        return quizId;
    }

    [Test]
    public async Task CreateRoom_GivenAuthenticatedHostWithPublishedQuiz_WhenCalled_ThenReturnsRoomCodeAndJoinsHostGroup()
    {
        var (hostClient, hostCookies, _) = await GameHubTestClient.CreateAuthenticatedHostClientAsync(_factory);
        using var _1 = hostClient;
        var quizId = await CreateAndPublishQuizAsync(hostClient);

        await using var hostConnection = await GameHubTestClient.CreateHostHubConnectionAsync(_factory, hostCookies);

        var roomCreatedTcs = new TaskCompletionSource<string>();
        hostConnection.On<string>("RoomCreated", roomCode => roomCreatedTcs.TrySetResult(roomCode));

        await hostConnection.InvokeAsync("CreateRoom", new CreateRoomRequest(quizId, null, null));

        var roomCode = await roomCreatedTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.That(roomCode, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task JoinRoom_GivenExistingRoomAndAnonymousPlayer_WhenCalled_ThenPlayerReceivesRoomStateAndHostIsNotified()
    {
        var (hostClient, hostCookies, _) = await GameHubTestClient.CreateAuthenticatedHostClientAsync(_factory);
        using var _1 = hostClient;
        var quizId = await CreateAndPublishQuizAsync(hostClient);

        await using var hostConnection = await GameHubTestClient.CreateHostHubConnectionAsync(_factory, hostCookies);

        var roomCreatedTcs = new TaskCompletionSource<string>();
        hostConnection.On<string>("RoomCreated", roomCode => roomCreatedTcs.TrySetResult(roomCode));
        var playerJoinedOnHostTcs = new TaskCompletionSource<PlayerDto>();
        hostConnection.On<PlayerDto>("PlayerJoined", player => playerJoinedOnHostTcs.TrySetResult(player));

        await hostConnection.InvokeAsync("CreateRoom", new CreateRoomRequest(quizId, null, null));
        var roomCode = await roomCreatedTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        await using var playerConnection = await GameHubTestClient.CreateAnonymousPlayerConnectionAsync(_factory);
        var roomStateSyncTcs = new TaskCompletionSource<RoomStateSyncDto>();
        playerConnection.On<RoomStateSyncDto>("RoomStateSync", state => roomStateSyncTcs.TrySetResult(state));

        await playerConnection.InvokeAsync("JoinRoom", roomCode, "Alice");

        var roomState = await roomStateSyncTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));
        var playerJoinedOnHost = await playerJoinedOnHostTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.That(roomState.Room.RoomCode, Is.EqualTo(roomCode));
        Assert.That(playerJoinedOnHost.Nickname, Is.EqualTo("Alice"));
    }

    private async Task<(HubConnection Host, HubConnection Player, string RoomCode, Guid PlayerId)> CreateRoomWithOnePlayerAsync()
    {
        var (hostClient, hostCookies, _) = await GameHubTestClient.CreateAuthenticatedHostClientAsync(_factory);
        using var _1 = hostClient;
        var quizId = await CreateAndPublishQuizAsync(hostClient);

        var hostConnection = await GameHubTestClient.CreateHostHubConnectionAsync(_factory, hostCookies);
        var roomCreatedTcs = new TaskCompletionSource<string>();
        hostConnection.On<string>("RoomCreated", roomCode => roomCreatedTcs.TrySetResult(roomCode));
        var playerJoinedOnHostTcs = new TaskCompletionSource<PlayerDto>();
        hostConnection.On<PlayerDto>("PlayerJoined", player => playerJoinedOnHostTcs.TrySetResult(player));
        await hostConnection.InvokeAsync("CreateRoom", new CreateRoomRequest(quizId, null, null));
        var roomCode = await roomCreatedTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        var playerConnection = await GameHubTestClient.CreateAnonymousPlayerConnectionAsync(_factory);
        var roomStateSyncTcs = new TaskCompletionSource<RoomStateSyncDto>();
        playerConnection.On<RoomStateSyncDto>("RoomStateSync", state => roomStateSyncTcs.TrySetResult(state));
        await playerConnection.InvokeAsync("JoinRoom", roomCode, "Alice");
        await roomStateSyncTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));
        var playerJoined = await playerJoinedOnHostTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        return (hostConnection, playerConnection, roomCode, playerJoined.Id);
    }

    [Test]
    public async Task StartGame_GivenRoomWithPlayer_WhenHostStarts_ThenGameStartedAndQuestionStartAreBroadcast()
    {
        var (hostConnection, playerConnection, roomCode, _) = await CreateRoomWithOnePlayerAsync();
        await using var _1 = hostConnection;
        await using var _2 = playerConnection;

        var gameStartedTcs = new TaskCompletionSource<bool>();
        playerConnection.On("GameStarted", () => gameStartedTcs.TrySetResult(true));
        var questionStartTcs = new TaskCompletionSource<QuestionPayloadDto>();
        playerConnection.On<QuestionPayloadDto>("QuestionStart", q => questionStartTcs.TrySetResult(q));

        await hostConnection.InvokeAsync("StartGame", roomCode);

        await gameStartedTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));
        var question = await questionStartTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.That(question.Text, Is.EqualTo("1 + 1 = ?"));
        Assert.That(question.Options, Has.Count.EqualTo(4));
    }

    [Test]
    public async Task SubmitAnswer_GivenStartedGame_WhenPlayerAnswers_ThenPlayerGetsResultAndHostGetsProgress()
    {
        var (hostConnection, playerConnection, roomCode, _) = await CreateRoomWithOnePlayerAsync();
        await using var _1 = hostConnection;
        await using var _2 = playerConnection;

        var questionStartTcs = new TaskCompletionSource<QuestionPayloadDto>();
        playerConnection.On<QuestionPayloadDto>("QuestionStart", q => questionStartTcs.TrySetResult(q));
        await hostConnection.InvokeAsync("StartGame", roomCode);
        await questionStartTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        var answerResultTcs = new TaskCompletionSource<AnswerResultDto>();
        playerConnection.On<AnswerResultDto>("AnswerResult", result => answerResultTcs.TrySetResult(result));
        var answerProgressTcs = new TaskCompletionSource<AnswerProgressDto>();
        hostConnection.On<AnswerProgressDto>("AnswerProgress", progress => answerProgressTcs.TrySetResult(progress));

        await playerConnection.InvokeAsync("SubmitAnswer", roomCode, 1);

        var answerResult = await answerResultTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));
        var answerProgress = await answerProgressTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.That(answerResult.IsCorrect, Is.True);
        Assert.That(answerResult.CorrectAnswerIndex, Is.EqualTo(1));
        Assert.That(answerProgress.Answered, Is.EqualTo(1));
        Assert.That(answerProgress.SelectedOptionIndex, Is.EqualTo(1));
    }

    [Test]
    public async Task SkipQuestion_GivenSingleQuestionQuizAlreadyStarted_WhenHostSkipsTwice_ThenQuestionEndThenGameEndAreBroadcast()
    {
        var (hostConnection, playerConnection, roomCode, _) = await CreateRoomWithOnePlayerAsync();
        await using var _1 = hostConnection;
        await using var _2 = playerConnection;

        var questionStartTcs = new TaskCompletionSource<QuestionPayloadDto>();
        playerConnection.On<QuestionPayloadDto>("QuestionStart", q => questionStartTcs.TrySetResult(q));
        await hostConnection.InvokeAsync("StartGame", roomCode);
        await questionStartTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        var questionEndTcs = new TaskCompletionSource<QuestionEndPayloadDto>();
        playerConnection.On<QuestionEndPayloadDto>("QuestionEnd", payload => questionEndTcs.TrySetResult(payload));

        await hostConnection.InvokeAsync("SkipQuestion", roomCode);
        var questionEnd = await questionEndTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.That(questionEnd.AnswerIndex, Is.EqualTo(1));

        var gameEndTcs = new TaskCompletionSource<IReadOnlyList<RankEntryDto>>();
        playerConnection.On<IReadOnlyList<RankEntryDto>>("GameEnd", rankings => gameEndTcs.TrySetResult(rankings));

        await hostConnection.InvokeAsync("SkipQuestion", roomCode);
        var rankings = await gameEndTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.That(rankings, Has.Count.EqualTo(1));
        Assert.That(rankings[0].Nickname, Is.EqualTo("Alice"));
    }

    [Test]
    public async Task PauseGame_GivenHostOfExistingRoom_WhenCalledTwice_ThenBothCallsSucceed()
    {
        var (hostConnection, playerConnection, roomCode, _) = await CreateRoomWithOnePlayerAsync();
        await using var _1 = hostConnection;
        await using var _2 = playerConnection;
        await hostConnection.InvokeAsync("StartGame", roomCode);

        Assert.DoesNotThrowAsync(async () => await hostConnection.InvokeAsync("PauseGame", roomCode));
        Assert.DoesNotThrowAsync(async () => await hostConnection.InvokeAsync("PauseGame", roomCode));
    }

    [Test]
    public async Task PauseGame_GivenNonHostConnection_WhenCalled_ThenThrowsHubException()
    {
        var (hostConnection, playerConnection, roomCode, _) = await CreateRoomWithOnePlayerAsync();
        await using var _1 = hostConnection;
        await using var _2 = playerConnection;

        Assert.ThrowsAsync<HubException>(async () => await playerConnection.InvokeAsync("PauseGame", roomCode));
    }

    [Test]
    public async Task RejoinRoom_GivenHostReconnectingAfterGameStarted_WhenCalled_ThenReturnsQuestionPhaseSnapshot()
    {
        var (hostClient, hostCookies, _) = await GameHubTestClient.CreateAuthenticatedHostClientAsync(_factory);
        using var _1 = hostClient;
        var quizId = await CreateAndPublishQuizAsync(hostClient);

        var firstHostConnection = await GameHubTestClient.CreateHostHubConnectionAsync(_factory, hostCookies);
        var roomCreatedTcs = new TaskCompletionSource<string>();
        firstHostConnection.On<string>("RoomCreated", roomCode => roomCreatedTcs.TrySetResult(roomCode));
        await firstHostConnection.InvokeAsync("CreateRoom", new CreateRoomRequest(quizId, null, null));
        var roomCode = await roomCreatedTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await firstHostConnection.InvokeAsync("StartGame", roomCode);
        await firstHostConnection.DisposeAsync();

        await using var rejoinedHostConnection = await GameHubTestClient.CreateHostHubConnectionAsync(_factory, hostCookies);
        var roomStateSyncTcs = new TaskCompletionSource<RoomStateSyncDto>();
        rejoinedHostConnection.On<RoomStateSyncDto>("RoomStateSync", state => roomStateSyncTcs.TrySetResult(state));

        await rejoinedHostConnection.InvokeAsync("RejoinRoom", roomCode);
        var state = await roomStateSyncTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.That(state.Phase, Is.EqualTo("question"));
        Assert.That(state.Room.RoomCode, Is.EqualTo(roomCode));
        Assert.That(state.CurrentQuestion, Is.Not.Null);
    }

    [Test]
    public async Task OnDisconnected_GivenPlayerInRoom_WhenPlayerConnectionStops_ThenOtherClientsReceivePlayerLeft()
    {
        var (hostConnection, playerConnection, roomCode, playerId) = await CreateRoomWithOnePlayerAsync();
        await using var _1 = hostConnection;

        var playerLeftTcs = new TaskCompletionSource<string>();
        hostConnection.On<string>("PlayerLeft", leftPlayerId => playerLeftTcs.TrySetResult(leftPlayerId));

        await playerConnection.DisposeAsync();

        var leftPlayerId = await playerLeftTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.That(leftPlayerId, Is.EqualTo(playerId.ToString()));
    }
}

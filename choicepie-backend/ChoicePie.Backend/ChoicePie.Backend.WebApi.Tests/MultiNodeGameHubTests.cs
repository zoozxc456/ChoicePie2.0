using System.Collections.Concurrent;
using ChoicePie.Backend.Application.GameRooms.Dtos;
using ChoicePie.Backend.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace ChoicePie.Backend.WebApi.Tests;

/// <summary>
/// 兩個 WebApplicationFactory 共用同一組 Postgres / Redis，代表負載平衡後面的兩台後端。
/// Host 與玩家刻意連到不同節點，驗證廣播會跨節點送達、同時寫入同一個房間時不會互相覆蓋。
/// </summary>
public sealed class MultiNodeGameHubTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);

    private PostgreSqlContainer _postgres = null!;
    private RedisContainer _redis = null!;
    private CustomWebApplicationFactory _nodeA = null!;
    private CustomWebApplicationFactory _nodeB = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _postgres = CustomWebApplicationFactory.CreatePostgresContainer();
        _redis = CustomWebApplicationFactory.CreateRedisContainer();
        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync());

        _nodeA = new CustomWebApplicationFactory(_postgres, _redis);
        _nodeB = new CustomWebApplicationFactory(_postgres, _redis);

        // 兩台同時啟動：啟動時的 migration / seed 必須排隊，不能互撞。
        await Task.WhenAll(_nodeA.InitializeAsync(), _nodeB.InitializeAsync());
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _nodeA.DisposeAsync();
        await _nodeB.DisposeAsync();
        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();
    }

    [Test]
    public async Task JoinRoom_GivenHostOnNodeAAndPlayerOnNodeB_WhenPlayerJoins_ThenHostOnOtherNodeIsNotified()
    {
        var (hostConnection, roomCode) = await CreateRoomOnNodeAAsync();
        await using var _ = hostConnection;
        var playerJoinedOnHost = new TaskCompletionSource<PlayerDto>();
        hostConnection.On<PlayerDto>("PlayerJoined", player => playerJoinedOnHost.TrySetResult(player));

        await using var player = await GameHubTestClient.CreateAnonymousPlayerConnectionAsync(_nodeB);
        await player.InvokeAsync<JoinRoomResponse>("JoinRoom", roomCode, "Alice");

        var joined = await playerJoinedOnHost.Task.WaitAsync(Timeout);
        Assert.That(joined.Nickname, Is.EqualTo("Alice"));
    }

    [Test]
    public async Task StartGame_GivenHostOnNodeAAndPlayerOnNodeB_WhenHostStarts_ThenPlayerOnOtherNodeReceivesQuestion()
    {
        var (hostConnection, roomCode) = await CreateRoomOnNodeAAsync();
        await using var _ = hostConnection;
        await using var player = await GameHubTestClient.CreateAnonymousPlayerConnectionAsync(_nodeB);
        var questionStart = new TaskCompletionSource<QuestionPayloadDto>();
        player.On<QuestionPayloadDto>("QuestionStart", q => questionStart.TrySetResult(q));
        await player.InvokeAsync<JoinRoomResponse>("JoinRoom", roomCode, "Alice");

        await hostConnection.InvokeAsync("StartGame", roomCode);

        var question = await questionStart.Task.WaitAsync(Timeout);
        Assert.That(question.Text, Is.EqualTo("1 + 1 = ?"));
    }

    [Test]
    public async Task SubmitAnswer_GivenPlayersSpreadAcrossNodes_WhenAllJoinAndAnswerAtOnce_ThenNoJoinOrAnswerIsLost()
    {
        const int playerCount = 12;
        var (hostConnection, roomCode) = await CreateRoomOnNodeAAsync();
        await using var _ = hostConnection;
        var questionEnd = new TaskCompletionSource<QuestionEndPayloadDto>();
        hostConnection.On<QuestionEndPayloadDto>("QuestionEnd", payload => questionEnd.TrySetResult(payload));

        var players = await Task.WhenAll(Enumerable.Range(0, playerCount).Select(i =>
            GameHubTestClient.CreateAnonymousPlayerConnectionAsync(i % 2 == 0 ? _nodeA : _nodeB)));
        try
        {
            var questionStarted = new ConcurrentBag<int>();
            var allStarted = new TaskCompletionSource();
            for (var i = 0; i < playerCount; i++)
            {
                var index = i;
                players[i].On<QuestionPayloadDto>("QuestionStart", _ =>
                {
                    questionStarted.Add(index);
                    if (questionStarted.Count == playerCount) allStarted.TrySetResult();
                });
            }

            // 全部同時加入：每次 JoinRoom 都是對同一個房間「讀取 → 加人 → 寫回」。
            await Task.WhenAll(players.Select((p, i) => p.InvokeAsync<JoinRoomResponse>("JoinRoom", roomCode, $"P{i:00}")));

            await hostConnection.InvokeAsync("StartGame", roomCode);
            await allStarted.Task.WaitAsync(Timeout);

            // 全部同時作答（都答對），再由 Host 結束這題，看結算時每個人的分數是否都還在。
            await Task.WhenAll(players.Select(p => p.InvokeAsync("SubmitAnswer", roomCode, 1)));
            await hostConnection.InvokeAsync("SkipQuestion", roomCode);

            var rankings = (await questionEnd.Task.WaitAsync(Timeout)).Rankings;
            Assert.That(rankings, Has.Count.EqualTo(playerCount), "同時加入的玩家不能被覆蓋掉");
            Assert.That(rankings.Where(r => r.Score == 0).Select(r => r.Nickname), Is.Empty,
                "同時作答的答案不能被覆蓋掉");
        }
        finally
        {
            foreach (var player in players)
            {
                await player.DisposeAsync();
            }
        }
    }

    [Test]
    public async Task BrowserLikeConnection_GivenHostOnNodeAAndPlayerOnNodeB_WhenGameIsPlayed_ThenWorksWithoutNegotiation()
    {
        var (hostClient, hostCookies, _) = await GameHubTestClient.CreateAuthenticatedHostClientAsync(_nodeA);
        using var _1 = hostClient;
        var quizId = await GameHubTestClient.CreateAndPublishQuizAsync(hostClient);

        await using var host = await GameHubTestClient.CreateBrowserLikeConnectionAsync(_nodeA, hostCookies);
        var roomCreated = new TaskCompletionSource<string>();
        var playerJoinedOnHost = new TaskCompletionSource<PlayerDto>();
        host.On<string>("RoomCreated", code => roomCreated.TrySetResult(code));
        host.On<PlayerDto>("PlayerJoined", p => playerJoinedOnHost.TrySetResult(p));
        // CreateRoom 是 MemberOnly：成功代表 WebSocket handshake 上的 cookie 有被認出身分。
        await host.InvokeAsync("CreateRoom", new CreateRoomRequest(quizId, null, null));
        var roomCode = await roomCreated.Task.WaitAsync(Timeout);

        await using var player = await GameHubTestClient.CreateBrowserLikeConnectionAsync(_nodeB);
        var questionStart = new TaskCompletionSource<QuestionPayloadDto>();
        player.On<QuestionPayloadDto>("QuestionStart", q => questionStart.TrySetResult(q));
        await player.InvokeAsync<JoinRoomResponse>("JoinRoom", roomCode, "Alice");
        Assert.That((await playerJoinedOnHost.Task.WaitAsync(Timeout)).Nickname, Is.EqualTo("Alice"));

        await host.InvokeAsync("StartGame", roomCode);

        Assert.That((await questionStart.Task.WaitAsync(Timeout)).Text, Is.EqualTo("1 + 1 = ?"));
    }

    [Test]
    public async Task BrowserLikeConnection_GivenNoAuthCookie_WhenCallingMemberOnlyMethod_ThenIsRejected()
    {
        await using var anonymous = await GameHubTestClient.CreateBrowserLikeConnectionAsync(_nodeB);

        Assert.ThrowsAsync<HubException>(() =>
            anonymous.InvokeAsync("CreateRoom", new CreateRoomRequest(Guid.NewGuid(), null, null)));
    }

    private async Task<(HubConnection Host, string RoomCode)> CreateRoomOnNodeAAsync()
    {
        var (hostClient, hostCookies, _) = await GameHubTestClient.CreateAuthenticatedHostClientAsync(_nodeA);
        using var _1 = hostClient;
        var quizId = await GameHubTestClient.CreateAndPublishQuizAsync(hostClient);

        var hostConnection = await GameHubTestClient.CreateHostHubConnectionAsync(_nodeA, hostCookies);
        var roomCreated = new TaskCompletionSource<string>();
        hostConnection.On<string>("RoomCreated", code => roomCreated.TrySetResult(code));
        await hostConnection.InvokeAsync("CreateRoom", new CreateRoomRequest(quizId, null, null));

        return (hostConnection, await roomCreated.Task.WaitAsync(Timeout));
    }
}

using ChoicePie.Backend.Application.GameRooms.Commands;
using ChoicePie.Backend.Application.GameRooms.Queries;
using ChoicePie.Backend.Shared.Kernel.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChoicePie.Backend.WebApi.Hubs;

/// <summary>
/// 對應 backend-spec.md 第 2 節：即時對戰的 SignalR Hub。
/// Player 不需登入（匿名連線 + JoinRoom 提供 nickname），Host 動作需要 JWT（MemberOnly）。
/// </summary>
public sealed class GameHub(IMediator mediator, ILogger<GameHub> logger) : Hub<IGameHubClient>
{
    private const string RoomCodeItemKey = "RoomCode";
    private const string PlayerIdItemKey = "PlayerId";
    private const string IsHostItemKey = "IsHost";

    [Authorize(Policy = "MemberOnly")]
    public async Task CreateRoom(CreateRoomRequest request)
    {
        var hostUserId = GetHostUserId();

        var result = await mediator.Send(new CreateRoomCommand
        {
            HostUserId = hostUserId,
            QuizId = request.QuizId,
            QuestionIds = request.QuestionIds ?? [],
            TimeLimitSeconds = request.TimeLimit ?? 20
        });

        await JoinHostGroupsAsync(result.RoomCode);

        await Clients.Caller.RoomCreated(result.RoomCode);
    }

    [Authorize(Policy = "MemberOnly")]
    public async Task StartGame(string roomCode)
    {
        var question = await mediator.Send(new StartGameCommand(roomCode, GetHostUserId()));

        await Clients.Group(RoomGroup(roomCode)).GameStarted();
        await Clients.Group(RoomGroup(roomCode)).QuestionStart(question);
    }

    [Authorize(Policy = "MemberOnly")]
    public async Task SkipQuestion(string roomCode)
    {
        var result = await mediator.Send(new SkipQuestionCommand(roomCode, GetHostUserId()));

        switch (result.Kind)
        {
            case SkipQuestionOutcomeKind.QuestionEnded:
                await Clients.Group(RoomGroup(roomCode)).QuestionEnd(result.QuestionEnd!);
                break;
            case SkipQuestionOutcomeKind.AdvancedToNextQuestion:
                await Clients.Group(RoomGroup(roomCode)).QuestionStart(result.NextQuestion!);
                break;
            case SkipQuestionOutcomeKind.GameEnded:
                await Clients.Group(RoomGroup(roomCode)).GameEnd(result.FinalRankings!);
                break;
        }
    }

    [Authorize(Policy = "MemberOnly")]
    public Task PauseGame(string roomCode) => mediator.Send(new PauseGameCommand(roomCode, GetHostUserId()));

    [Authorize(Policy = "MemberOnly")]
    public async Task RejoinRoom(string roomCode)
    {
        var sync = await mediator.Send(new RejoinRoomQuery(roomCode, GetHostUserId()));

        await JoinHostGroupsAsync(roomCode);

        await Clients.Caller.RoomStateSync(sync);
    }

    public async Task JoinRoom(string roomCode, string nickname)
    {
        var result = await mediator.Send(new JoinRoomCommand(roomCode, nickname, Context.ConnectionId, GetOptionalMemberId()));

        Context.Items[RoomCodeItemKey] = roomCode;
        Context.Items[PlayerIdItemKey] = result.Player.Id;

        await Groups.AddToGroupAsync(Context.ConnectionId, RoomGroup(roomCode));
        await Clients.Caller.RoomStateSync(result.RoomState);
        await Clients.Group(RoomGroup(roomCode)).PlayerJoined(result.Player);
    }

    public async Task SubmitAnswer(string roomCode, int answerIndex)
    {
        var playerId = GetPlayerId();

        var result = await mediator.Send(new SubmitAnswerCommand(roomCode, playerId, answerIndex));

        await Clients.Caller.AnswerResult(result.Result);
        await Clients.Group(HostGroup(roomCode)).AnswerProgress(result.Progress);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is null)
        {
            logger.LogInformation("Hub 連線正常關閉，ConnectionId={ConnectionId}", Context.ConnectionId);
        }
        else
        {
            logger.LogWarning(exception, "Hub 連線異常斷開，ConnectionId={ConnectionId}", Context.ConnectionId);
        }

        var isHost = Context.Items.TryGetValue(IsHostItemKey, out var isHostValue) && isHostValue is true;

        if (!isHost && Context.Items.TryGetValue(RoomCodeItemKey, out var roomCodeValue) &&
            roomCodeValue is string roomCode)
        {
            var playerId = Context.Items.TryGetValue(PlayerIdItemKey, out var playerIdValue) && playerIdValue is Guid id
                ? id.ToString()
                : Context.ConnectionId;

            await Clients.Group(RoomGroup(roomCode)).PlayerLeft(playerId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private async Task JoinHostGroupsAsync(string roomCode)
    {
        Context.Items[RoomCodeItemKey] = roomCode;
        Context.Items[IsHostItemKey] = true;

        await Groups.AddToGroupAsync(Context.ConnectionId, RoomGroup(roomCode));
        await Groups.AddToGroupAsync(Context.ConnectionId, HostGroup(roomCode));
    }

    private Guid GetHostUserId()
    {
        if (Context.User?.FindFirst(JwtClaimValues.RoleClaimType)?.Value != JwtClaimValues.MemberRole)
        {
            throw new HubException("未登入或無 Host 權限。");
        }

        var value = Context.User.FindFirst("sub")?.Value;
        return Guid.TryParse(value, out var userId) ? userId : throw new HubException("無效的使用者身分。");
    }

    /// <summary>
    /// JoinRoom 不需登入即可呼叫，但若連線已帶有效 JWT cookie（玩家其實有登入），
    /// 順便記下 MemberId，讓遊戲結束後的 GameSession 可歸戶到「我玩過的遊戲」。
    /// </summary>
    private Guid? GetOptionalMemberId()
    {
        if (Context.User?.FindFirst(JwtClaimValues.RoleClaimType)?.Value != JwtClaimValues.MemberRole)
        {
            return null;
        }

        var value = Context.User.FindFirst("sub")?.Value;
        return Guid.TryParse(value, out var userId) ? userId : null;
    }

    private Guid GetPlayerId() =>
        Context.Items.TryGetValue(PlayerIdItemKey, out var value) && value is Guid playerId
            ? playerId
            : throw new HubException("尚未加入房間。");

    private static string RoomGroup(string roomCode) => $"room:{roomCode}";
    private static string HostGroup(string roomCode) => $"room:{roomCode}:host";
}

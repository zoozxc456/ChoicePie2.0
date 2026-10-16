using ChoicePie.Backend.Application.GameRooms.Dtos;

namespace ChoicePie.Backend.WebApi.Hubs;

/// <summary>Server → Client 事件，對應 backend-spec.md 第 2.2 節。</summary>
public interface IGameHubClient
{
    Task RoomCreated(string roomCode);
    Task PlayerJoined(PlayerDto player);
    /// <summary>大廳階段玩家離開（已從房間移除）；參數為 playerId。</summary>
    Task PlayerLeft(string playerId);
    /// <summary>遊戲開始後玩家斷線（保留分數、僅標記離線）；參數為 playerId。</summary>
    Task PlayerDisconnected(Guid playerId);
    /// <summary>離線玩家已用重連憑證回來；payload 為公開的 PlayerDto（不含憑證）。</summary>
    Task PlayerReconnected(PlayerDto player);
    /// <summary>Host 切換暫停/繼續；整個房間（Host、玩家、大螢幕）都會收到。</summary>
    Task PauseStateChanged(PauseStateDto payload);
    /// <summary>Host 連線狀態變化（Connected=false 表示離線）；整個房間都會收到。</summary>
    Task HostConnectionChanged(HostConnectionStateDto payload);
    Task AnswerProgress(AnswerProgressDto payload);
    Task GameStarted();
    Task QuestionStart(QuestionPayloadDto payload);
    Task AnswerResult(AnswerResultDto payload);
    Task QuestionEnd(QuestionEndPayloadDto payload);
    Task GameEnd(IReadOnlyList<RankEntryDto> rankings);
    Task RoomStateSync(RoomStateSyncDto payload);
}

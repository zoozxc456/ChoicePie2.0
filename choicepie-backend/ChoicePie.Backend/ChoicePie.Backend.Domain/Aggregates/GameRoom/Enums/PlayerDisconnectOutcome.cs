namespace ChoicePie.Backend.Domain.Aggregates.GameRoom.Enums;

/// <summary>玩家斷線處理結果：大廳階段直接移除、遊戲開始後只標記離線、過期的斷線通知則忽略。</summary>
public enum PlayerDisconnectOutcome
{
    Ignored,
    Removed,
    MarkedDisconnected
}

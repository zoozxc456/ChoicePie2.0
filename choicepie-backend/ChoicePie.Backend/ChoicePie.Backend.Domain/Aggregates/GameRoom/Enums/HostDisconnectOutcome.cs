namespace ChoicePie.Backend.Domain.Aggregates.GameRoom.Enums;

/// <summary>Host 連線中斷處理結果：過期連線忽略、只標記離線、或標記離線並自動暫停題目。</summary>
public enum HostDisconnectOutcome
{
    Ignored,
    MarkedOffline,
    MarkedOfflineAndPaused
}

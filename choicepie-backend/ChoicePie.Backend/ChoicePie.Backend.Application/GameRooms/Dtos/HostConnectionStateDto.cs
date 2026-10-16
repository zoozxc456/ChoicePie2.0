namespace ChoicePie.Backend.Application.GameRooms.Dtos;

/// <summary>Host 連線狀態事件 payload：Connected=false 表示 Host 離線，玩家與大螢幕應顯示等待提示。</summary>
public sealed record HostConnectionStateDto(bool Connected);

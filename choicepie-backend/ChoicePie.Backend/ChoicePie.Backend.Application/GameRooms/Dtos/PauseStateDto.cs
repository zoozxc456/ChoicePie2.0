namespace ChoicePie.Backend.Application.GameRooms.Dtos;

/// <summary>暫停狀態事件 payload：RemainingMs 為伺服器計算的當前題目剩餘毫秒數。</summary>
public sealed record PauseStateDto(bool IsPaused, long RemainingMs);

namespace ChoicePie.Backend.Application.GameRooms;

/// <summary>
/// 會改寫既有房間狀態的指令。<see cref="GameRoomLockBehavior{TRequest,TResponse}"/> 依這個標記
/// 以房間為單位上分散式鎖，讓「讀取 → 修改 → 整份寫回」在多台後端之間也是序列化的。
/// </summary>
public interface IGameRoomCommand
{
    string RoomCode { get; }
}

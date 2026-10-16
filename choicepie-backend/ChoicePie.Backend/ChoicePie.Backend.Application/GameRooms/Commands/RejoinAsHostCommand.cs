using ChoicePie.Backend.Application.GameRooms.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

/// <summary>Host 回到房間：記下新的 Host 連線、標記上線，回傳房間快照。不會自動繼續遊戲。</summary>
public sealed record RejoinAsHostCommand(string RoomCode, Guid HostUserId, string ConnectionId)
    : IRequest<RoomStateSyncDto>;

using ChoicePie.Backend.Application.GameRooms.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Queries;

/// <summary>大螢幕（旁觀者）取得房間快照：不含 self、不含選項明細，也不算玩家。</summary>
public sealed record WatchRoomQuery(string RoomCode) : IRequest<RoomStateSyncDto>;

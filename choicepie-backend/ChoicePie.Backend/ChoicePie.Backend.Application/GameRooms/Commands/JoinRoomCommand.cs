using ChoicePie.Backend.Application.GameRooms.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public sealed record JoinRoomCommand(string RoomCode, string Nickname, string ConnectionId, Guid? MemberId = null) : IRequest<JoinRoomResult>;

/// <summary>ReconnectToken 只回給呼叫者本人，不可放進任何廣播 payload。</summary>
public sealed record JoinRoomResult(PlayerDto Player, RoomStateSyncDto RoomState, string ReconnectToken);

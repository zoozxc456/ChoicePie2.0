using ChoicePie.Backend.Application.GameRooms.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public sealed record RejoinAsPlayerCommand(string RoomCode, string ReconnectToken, string ConnectionId)
    : IRequest<RejoinAsPlayerResult>, IGameRoomCommand;

/// <summary>Player 為對全房間廣播「玩家已重連」用的公開資料（不含重連憑證）；RoomState 只回給呼叫者。</summary>
public sealed record RejoinAsPlayerResult(PlayerDto Player, RoomStateSyncDto RoomState);

using ChoicePie.Backend.Application.GameRooms.Dtos;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Enums;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public sealed record DisconnectHostCommand(string RoomCode, string ConnectionId) : IRequest<HostDisconnectResultDto>, IGameRoomCommand;

/// <summary>Pause 只在「這次斷線觸發了自動暫停」時有值，供 Hub 廣播 PauseStateChanged。</summary>
public sealed record HostDisconnectResultDto(HostDisconnectOutcome Outcome, PauseStateDto? Pause);

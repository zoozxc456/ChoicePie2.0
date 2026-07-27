using ChoicePie.Backend.Application.GameRooms.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public sealed record JoinRoomCommand(string RoomCode, string Nickname, string ConnectionId, Guid? MemberId = null) : IRequest<JoinRoomResult>;

public sealed record JoinRoomResult(PlayerDto Player, RoomStateSyncDto RoomState);

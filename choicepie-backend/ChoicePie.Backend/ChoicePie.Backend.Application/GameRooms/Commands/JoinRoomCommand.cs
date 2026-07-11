using ChoicePie.Backend.Application.GameRooms.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public sealed record JoinRoomCommand(string RoomCode, string Nickname, string ConnectionId) : IRequest<PlayerDto>;

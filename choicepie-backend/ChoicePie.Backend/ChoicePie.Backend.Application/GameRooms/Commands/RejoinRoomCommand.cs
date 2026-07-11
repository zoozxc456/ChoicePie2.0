using ChoicePie.Backend.Application.GameRooms.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public sealed record RejoinRoomCommand(string RoomCode, Guid HostUserId) : IRequest<RoomStateSyncDto>;

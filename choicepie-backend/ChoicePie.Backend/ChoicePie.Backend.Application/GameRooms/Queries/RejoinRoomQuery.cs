using ChoicePie.Backend.Application.GameRooms.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Queries;

public sealed record RejoinRoomQuery(string RoomCode, Guid HostUserId) : IRequest<RoomStateSyncDto>;

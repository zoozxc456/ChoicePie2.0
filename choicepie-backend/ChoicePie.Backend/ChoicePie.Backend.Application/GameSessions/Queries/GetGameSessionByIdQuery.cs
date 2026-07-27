using ChoicePie.Backend.Application.GameSessions.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.GameSessions.Queries;

public sealed record GetGameSessionByIdQuery(Guid Id) : IRequest<GameSessionDetailDto>;

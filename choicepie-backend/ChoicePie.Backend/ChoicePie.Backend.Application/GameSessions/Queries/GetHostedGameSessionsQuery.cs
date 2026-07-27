using ChoicePie.Backend.Application.GameSessions.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.GameSessions.Queries;

public sealed record GetHostedGameSessionsQuery(int PageNumber, int PageSize) : IRequest<PagedResult<GameSessionSummaryDto>>;

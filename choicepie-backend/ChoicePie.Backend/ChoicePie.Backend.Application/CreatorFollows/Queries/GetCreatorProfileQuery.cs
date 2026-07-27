using ChoicePie.Backend.Application.CreatorFollows.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.CreatorFollows.Queries;

public sealed record GetCreatorProfileQuery(Guid CreatorId) : IRequest<CreatorProfileDto>;

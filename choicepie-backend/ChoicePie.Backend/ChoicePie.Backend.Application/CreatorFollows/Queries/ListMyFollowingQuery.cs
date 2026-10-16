using ChoicePie.Backend.Application.CreatorFollows.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.CreatorFollows.Queries;

public sealed record ListMyFollowingQuery(int PageNumber, int PageSize) : IRequest<PagedResult<FollowedCreatorDto>>;

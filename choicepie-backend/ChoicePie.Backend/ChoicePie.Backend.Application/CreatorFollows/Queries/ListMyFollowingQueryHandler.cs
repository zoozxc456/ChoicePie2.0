using ChoicePie.Backend.Application.CreatorFollows.Contracts;
using ChoicePie.Backend.Application.CreatorFollows.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.CreatorFollows.Queries;

public sealed class ListMyFollowingQueryHandler(
    ICreatorQueryService creatorQueryService,
    ICurrentUserService currentUserService)
    : IRequestHandler<ListMyFollowingQuery, PagedResult<FollowedCreatorDto>>
{
    public Task<PagedResult<FollowedCreatorDto>> Handle(ListMyFollowingQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthenticatedException();
        return creatorQueryService.ListFollowingAsync(userId, request.PageNumber, request.PageSize, cancellationToken);
    }
}

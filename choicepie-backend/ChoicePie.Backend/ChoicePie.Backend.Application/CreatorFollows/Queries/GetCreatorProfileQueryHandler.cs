using ChoicePie.Backend.Application.CreatorFollows.Contracts;
using ChoicePie.Backend.Application.CreatorFollows.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.CreatorFollows.Queries;

public sealed class GetCreatorProfileQueryHandler(
    ICreatorQueryService creatorQueryService,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetCreatorProfileQuery, CreatorProfileDto>
{
    public async Task<CreatorProfileDto> Handle(GetCreatorProfileQuery request, CancellationToken cancellationToken) =>
        await creatorQueryService.GetByIdAsync(request.CreatorId, currentUserService.UserId, cancellationToken)
        ?? throw new MemberNotFoundException(request.CreatorId);
}

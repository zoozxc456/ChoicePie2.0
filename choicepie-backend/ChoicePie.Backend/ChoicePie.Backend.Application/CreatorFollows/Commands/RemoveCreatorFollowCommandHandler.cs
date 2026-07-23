using ChoicePie.Backend.Domain.Aggregates.CreatorFollow;
using ChoicePie.Backend.Domain.Aggregates.CreatorFollow.Specifications;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.CreatorFollows.Commands;

public sealed class RemoveCreatorFollowCommandHandler(
    ICreatorFollowRepository followRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RemoveCreatorFollowCommand>
{
    public async Task Handle(RemoveCreatorFollowCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthenticatedException();

        var follow = await followRepository.FirstOrDefaultAsync(
            new CreatorFollowByFollowerAndCreatorSpecification(userId, request.CreatorId), cancellationToken);

        if (follow is null)
        {
            return;
        }

        await followRepository.DeleteAsync(follow, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

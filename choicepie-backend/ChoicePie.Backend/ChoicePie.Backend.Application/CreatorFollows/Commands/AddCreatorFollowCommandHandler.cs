using ChoicePie.Backend.Domain.Aggregates.CreatorFollow;
using ChoicePie.Backend.Domain.Aggregates.CreatorFollow.Specifications;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Application.CreatorFollows.Commands;

public sealed class AddCreatorFollowCommandHandler(
    ICreatorFollowRepository followRepository,
    IMemberRepository memberRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddCreatorFollowCommand>
{
    public async Task Handle(AddCreatorFollowCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthenticatedException();

        _ = await memberRepository.GetByIdAsync(request.CreatorId, cancellationToken)
            ?? throw new MemberNotFoundException(request.CreatorId);

        var alreadyFollowing = await followRepository.ExistsAsync(
            new CreatorFollowByFollowerAndCreatorSpecification(userId, request.CreatorId), cancellationToken);

        if (alreadyFollowing)
        {
            return;
        }

        var follow = CreatorFollow.Create(userId, request.CreatorId);
        await followRepository.AddAsync(follow, cancellationToken);
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException)
        {
            // 連點追蹤時另一個請求已經追蹤成功：結果相同，視為成功。
        }
    }
}

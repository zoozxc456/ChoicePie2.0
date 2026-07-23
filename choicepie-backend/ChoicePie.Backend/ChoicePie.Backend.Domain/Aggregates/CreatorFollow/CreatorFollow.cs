using ChoicePie.Backend.Domain.Aggregates.CreatorFollow.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.CreatorFollow;

public sealed class CreatorFollow : AggregateRoot<Guid>
{
    public Guid FollowedCreatorId { get; private set; }
    public Guid FollowerId => CreatorId!.Value;

    private CreatorFollow()
    {
    }

    public static CreatorFollow Create(Guid followerId, Guid creatorId)
    {
        if (followerId == creatorId)
        {
            throw new CannotFollowSelfException(followerId);
        }

        var follow = new CreatorFollow
        {
            Id = Guid.NewGuid(),
            FollowedCreatorId = creatorId
        };

        follow.SetCreated(followerId);

        return follow;
    }
}

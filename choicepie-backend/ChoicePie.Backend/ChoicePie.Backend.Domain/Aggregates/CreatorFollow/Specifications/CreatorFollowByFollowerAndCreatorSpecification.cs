using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.CreatorFollow.Specifications;

public sealed class CreatorFollowByFollowerAndCreatorSpecification(Guid followerId, Guid creatorId)
    : Specification<CreatorFollow>(f => f.CreatorId == followerId && f.FollowedCreatorId == creatorId);

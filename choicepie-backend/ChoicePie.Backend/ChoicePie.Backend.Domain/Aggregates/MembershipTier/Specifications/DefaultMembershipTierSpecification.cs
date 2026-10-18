using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.MembershipTier.Specifications;

public sealed class DefaultMembershipTierSpecification() : Specification<MembershipTier>(t => t.IsDefault);

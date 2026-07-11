using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;

public sealed class AuthAccountByMemberIdSpecification(Guid memberId) : Specification<AuthAccount>(a => a.MemberId == memberId);

using ChoicePie.Backend.Shared.Kernel.Primitives;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;

namespace ChoicePie.Backend.Domain.Aggregates.Member.Specifications;

public sealed class MemberByEmailSpecification(Email email) : Specification<Member>(member => member.Email == email);

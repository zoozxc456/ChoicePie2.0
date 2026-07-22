using ChoicePie.Backend.Shared.Kernel.Primitives;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;

namespace ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;

public sealed class AuthAccountByEmailSpecification(Email email) : Specification<AuthAccount>(a => a.Email == email);

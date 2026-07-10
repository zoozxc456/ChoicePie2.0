using ChoicePie.Backend.Shared.Kernel.Primitives;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;

namespace ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Specifications;

public sealed class AdminAuthAccountByEmailSpecification(Email email) : Specification<AdminAuthAccount>(a => a.Email == email);

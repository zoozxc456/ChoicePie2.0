using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Specifications;

public sealed class AdminAuthAccountByAdminUserIdSpecification(Guid adminUserId) : Specification<AdminAuthAccount>(a => a.AdminUserId == adminUserId);

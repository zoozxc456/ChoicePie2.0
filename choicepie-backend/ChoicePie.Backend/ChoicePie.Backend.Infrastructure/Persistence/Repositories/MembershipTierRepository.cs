using ChoicePie.Backend.Domain.Aggregates.MembershipTier;
using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using Microsoft.EntityFrameworkCore;

namespace ChoicePie.Backend.Infrastructure.Persistence.Repositories;

public sealed class MembershipTierRepository(ChoicePieDbContext context)
    : EfGenericRepository<MembershipTier, ChoicePieDbContext>(context), IMembershipTierRepository, IScopedDependency
{
    public override Task<MembershipTier?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Set<MembershipTier>().FirstOrDefaultAsync(t => t.Id == id, ct);
}

using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount;
using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using Microsoft.EntityFrameworkCore;

namespace ChoicePie.Backend.Infrastructure.Persistence.Repositories;

public sealed class AdminAuthAccountRepository(ChoicePieDbContext context)
    : EfGenericRepository<AdminAuthAccount, ChoicePieDbContext>(context), IAdminAuthAccountRepository,
        IScopedDependency
{
    protected override IQueryable<AdminAuthAccount> ApplyInclude(IQueryable<AdminAuthAccount> queryable) =>
        queryable.Include(a => a.LoginMethods);

    public override Task<AdminAuthAccount?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        ApplyInclude(context.Set<AdminAuthAccount>()).FirstOrDefaultAsync(a => a.Id == id, ct);
}

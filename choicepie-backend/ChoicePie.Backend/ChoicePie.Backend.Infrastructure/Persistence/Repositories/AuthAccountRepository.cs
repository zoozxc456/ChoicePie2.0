using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using Microsoft.EntityFrameworkCore;

namespace ChoicePie.Backend.Infrastructure.Persistence.Repositories;

public sealed class AuthAccountRepository(ChoicePieDbContext context)
    : EfGenericRepository<AuthAccount, ChoicePieDbContext>(context), IAuthAccountRepository, IScopedDependency
{
    protected override IQueryable<AuthAccount> ApplyInclude(IQueryable<AuthAccount> queryable) =>
        queryable.Include(a => a.LoginMethods);

    public override Task<AuthAccount?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        ApplyInclude(context.Set<AuthAccount>()).FirstOrDefaultAsync(a => a.Id == id, ct);
}

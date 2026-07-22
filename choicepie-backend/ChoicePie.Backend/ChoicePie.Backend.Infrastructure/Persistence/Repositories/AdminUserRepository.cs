using ChoicePie.Backend.Domain.Aggregates.AdminUser;
using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using Microsoft.EntityFrameworkCore;

namespace ChoicePie.Backend.Infrastructure.Persistence.Repositories;

public sealed class AdminUserRepository(ChoicePieDbContext context)
    : EfGenericRepository<AdminUser, ChoicePieDbContext>(context), IAdminUserRepository, IScopedDependency
{
    public override Task<AdminUser?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Set<AdminUser>().FirstOrDefaultAsync(a => a.Id == id, ct);
}

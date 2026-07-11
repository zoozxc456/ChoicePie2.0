using ChoicePie.Backend.Domain.Aggregates.RefreshToken;
using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using Microsoft.EntityFrameworkCore;
using RefreshTokenAggregate = ChoicePie.Backend.Domain.Aggregates.RefreshToken.RefreshToken;

namespace ChoicePie.Backend.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository(ChoicePieDbContext context)
    : EfGenericRepository<RefreshTokenAggregate, ChoicePieDbContext>(context), IRefreshTokenRepository,
        IScopedDependency
{
    public override Task<RefreshTokenAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Set<RefreshTokenAggregate>().FirstOrDefaultAsync(t => t.Id == id, ct);
}

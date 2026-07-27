using ChoicePie.Backend.Domain.Aggregates.CreatorFollow;
using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using Microsoft.EntityFrameworkCore;

namespace ChoicePie.Backend.Infrastructure.Persistence.Repositories;

public sealed class CreatorFollowRepository(ChoicePieDbContext context)
    : EfGenericRepository<CreatorFollow, ChoicePieDbContext>(context), ICreatorFollowRepository, IScopedDependency
{
    public override Task<CreatorFollow?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Set<CreatorFollow>().FirstOrDefaultAsync(f => f.Id == id, ct);
}

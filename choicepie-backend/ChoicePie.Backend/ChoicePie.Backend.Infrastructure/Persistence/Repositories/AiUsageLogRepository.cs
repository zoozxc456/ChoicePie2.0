using ChoicePie.Backend.Domain.Aggregates.AiUsageLog;
using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using Microsoft.EntityFrameworkCore;

namespace ChoicePie.Backend.Infrastructure.Persistence.Repositories;

public sealed class AiUsageLogRepository(ChoicePieDbContext context)
    : EfGenericRepository<AiUsageLog, ChoicePieDbContext>(context), IAiUsageLogRepository, IScopedDependency
{
    public override Task<AiUsageLog?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Set<AiUsageLog>().FirstOrDefaultAsync(l => l.Id == id, ct);
}

using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using Microsoft.EntityFrameworkCore;
using GameSessionAggregate = ChoicePie.Backend.Domain.Aggregates.GameSession.GameSession;

namespace ChoicePie.Backend.Infrastructure.Persistence.Repositories;

public sealed class GameSessionRepository(ChoicePieDbContext context)
    : EfGenericRepository<GameSessionAggregate, ChoicePieDbContext>(context),
        ChoicePie.Backend.Domain.Aggregates.GameSession.IGameSessionRepository, IScopedDependency
{
    protected override IQueryable<GameSessionAggregate> ApplyInclude(IQueryable<GameSessionAggregate> queryable) =>
        queryable.Include(s => s.Questions).Include(s => s.PlayerResults);

    public override Task<GameSessionAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        ApplyInclude(context.Set<GameSessionAggregate>()).FirstOrDefaultAsync(s => s.Id == id, ct);
}

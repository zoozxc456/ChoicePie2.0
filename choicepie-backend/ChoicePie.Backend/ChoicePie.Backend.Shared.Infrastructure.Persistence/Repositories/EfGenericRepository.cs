using ChoicePie.Backend.Shared.Infrastructure.Persistence.Specifications;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;

public abstract class EfGenericRepository<TEntity, TContext>(TContext context)
    : IRepository<TEntity>
    where TEntity : class
    where TContext : DbContext
{
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();
    private readonly IQueryable<TEntity> _queryable = context.Set<TEntity>().AsQueryable();

    protected virtual IQueryable<TEntity> ApplyInclude(IQueryable<TEntity> queryable)
    {
        return queryable;
    }

    public abstract Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);

    public async Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
    {
        await _dbSet.AddRangeAsync(entities, ct);
    }

    public Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TEntity entity, CancellationToken ct = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public Task<TEntity?> GetOneAsync(CancellationToken ct = default)
        => ApplyInclude(_queryable).FirstOrDefaultAsync(ct);

    public Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken ct = default)
        => SpecificationEvaluator.Apply(ApplyInclude(_queryable), specification).FirstOrDefaultAsync(ct);

    public Task<List<TEntity>> ListAsync(ISpecification<TEntity> specification, CancellationToken ct = default)
        => SpecificationEvaluator.Apply(ApplyInclude(_queryable), specification).ToListAsync(ct);

    public Task<List<TEntity>> GetAllAsync(CancellationToken ct = default)
        => ApplyInclude(_queryable).ToListAsync(ct);

    public Task<bool> ExistsAsync(ISpecification<TEntity> specification, CancellationToken ct = default)
        => SpecificationEvaluator.Apply(ApplyInclude(_queryable), specification).AnyAsync(ct);

    public Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken ct = default)
        => SpecificationEvaluator.Apply(ApplyInclude(_queryable), specification).CountAsync(ct);
}
using System.Linq.Expressions;
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


    public Task<TEntity?> GetOneAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        => ApplyInclude(_queryable).FirstOrDefaultAsync(predicate, ct);

    public Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        => ApplyInclude(_queryable).Where(predicate).ToListAsync(ct);

    public Task<List<TEntity>> GetAllAsync(CancellationToken ct = default)
        => ApplyInclude(_queryable).ToListAsync(ct);

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        => ApplyInclude(_queryable).AnyAsync(predicate, ct);

    public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        => ApplyInclude(_queryable).CountAsync(predicate, ct);
}
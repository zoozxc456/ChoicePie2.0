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

    /// <summary>
    /// Command handler 的慣例是先用 GetByIdAsync 拿到「已被同一個 DbContext 追蹤」的聚合根直接在記憶體修改，
    /// 呼叫 SaveChangesAsync 前不需要（也不應該）再呼叫 Update——對已追蹤的實體呼叫 context.Update() 會重新
    /// 走一遍整個物件圖，把「Id 已經是 client 端產生的非預設 Guid、但其實還沒存進 DB」的新增子實體
    /// （例如剛 AddQuestion() 出來的 Question）誤判成「已存在」而標成 Modified，
    /// 導致存檔時對不存在的 row 送出 UPDATE，直接炸出 DbUpdateConcurrencyException。
    /// 只有在實體真的是 Detached（呼叫端傳進一個沒被目前 context 追蹤過的物件）時才需要重新掛回追蹤。
    /// </summary>
    public Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        if (context.Entry(entity).State == EntityState.Detached)
        {
            _dbSet.Update(entity);
        }

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
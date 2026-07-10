namespace ChoicePie.Backend.Shared.Kernel.Abstractions.Data;

public interface IRepository<TEntity>
{
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);
    Task UpdateAsync(TEntity entity, CancellationToken ct = default);
    Task DeleteAsync(TEntity entity, CancellationToken ct = default);
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TEntity?> GetOneAsync(CancellationToken ct = default);
    Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken ct = default);
    Task<List<TEntity>> ListAsync(ISpecification<TEntity> specification, CancellationToken ct = default);
    Task<List<TEntity>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsAsync(ISpecification<TEntity> specification, CancellationToken ct = default);
    Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken ct = default);
}
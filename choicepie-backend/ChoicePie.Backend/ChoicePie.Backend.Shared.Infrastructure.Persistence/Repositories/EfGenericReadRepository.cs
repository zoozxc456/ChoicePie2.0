using ChoicePie.Backend.Shared.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;

public abstract class EfGenericReadRepository<TContext>(TContext context) : IReadRepository
    where TContext : DbContext
{
    public IQueryable<T> Query<T>() where T : class
    {
        return context.Set<T>().AsNoTracking();
    }
}
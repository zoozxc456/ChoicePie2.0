namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;

public interface IReadRepository
{
    IQueryable<T> Query<T>() where T : class;
}

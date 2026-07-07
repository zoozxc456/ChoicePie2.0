namespace ChoicePie.Backend.Shared.Application.Interfaces;

public interface IReadRepository
{
    IQueryable<T> Query<T>() where T : class;
}
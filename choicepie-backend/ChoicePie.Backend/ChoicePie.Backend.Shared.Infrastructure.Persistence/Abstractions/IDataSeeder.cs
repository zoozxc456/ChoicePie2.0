namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Abstractions;

public interface IDataSeeder
{
    public int Order { get; }
    public Task SeedAsync();
}
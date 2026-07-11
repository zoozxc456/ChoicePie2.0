using Microsoft.EntityFrameworkCore;

namespace ChoicePie.Backend.Infrastructure.Persistence.Contexts;

public class ChoicePieDbContext : DbContext
{
    public ChoicePieDbContext(DbContextOptions<ChoicePieDbContext> options) : base(options)
    {
    }

    protected ChoicePieDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChoicePieDbContext).Assembly);
    }
}
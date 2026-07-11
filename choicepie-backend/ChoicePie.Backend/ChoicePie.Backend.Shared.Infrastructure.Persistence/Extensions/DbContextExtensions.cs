using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Extensions;

public static class DbContextExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddNpgsqlServerPool<TContext>(string? connectionString)
            where TContext : DbContext
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Postgres SQL connection string not found.");
            }

            services.AddDbContextPool<TContext>(options =>
            {
                options.UseNpgsql(connectionString);
                options.UseSnakeCaseNamingConvention();
            });

            return services;
        }
    }
}
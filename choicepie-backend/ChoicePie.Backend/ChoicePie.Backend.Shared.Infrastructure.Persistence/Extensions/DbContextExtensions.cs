using ChoicePie.Backend.Shared.Infrastructure.Persistence.AppSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Extensions;

public static class DbContextExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// 連線字串刻意延遲到 DbContextOptions 真正建構當下（透過 IServiceProvider 解析 IConfiguration）才讀取，
        /// 而不是在這個註冊方法呼叫當下就同步擷取字串——否則像 WebApplicationFactory 這種在 builder.Build()
        /// 之後才疊加的設定覆寫（例如測試用 Testcontainers 連線字串）永遠疊不進來，會悄悄接回原本的設定來源。
        /// </summary>
        public IServiceCollection AddNpgsqlServerPool<TContext>()
            where TContext : DbContext
        {
            services.AddDbContextPool<TContext>((serviceProvider, options) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var connectionString = GetConnectionString(configuration);

                options.UseNpgsql(connectionString);
                options.UseSnakeCaseNamingConvention();
            });

            return services;
        }
    }

    private static string GetConnectionString(IConfiguration configuration)
    {
        var connections = configuration.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>();
        var connectionString = connections?.FirstOrDefault()?.ConnectionString;

        return string.IsNullOrEmpty(connectionString)
            ? throw new InvalidOperationException("Postgres SQL connection string not found.")
            : connectionString;
    }
}
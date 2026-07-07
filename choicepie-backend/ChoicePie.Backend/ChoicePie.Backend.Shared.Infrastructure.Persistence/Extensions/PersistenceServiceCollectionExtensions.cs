using ChoicePie.Backend.Shared.Infrastructure.Persistence.AppSettings;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Extensions;

public static class PersistenceServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSharedPersistence<TContext>()
            where TContext : DbContext
        {
            services.AddScoped<IUnitOfWork, EfUnitOfWork<TContext>>();
            return services;
        }

        public IServiceCollection AddSharedDbContextPool<TContext>(IConfiguration configuration)
            where TContext : DbContext
        {
            var connection = GetValidConnection(configuration);
            RegisterDatabaseProvider<TContext>(services, connection);

            return services;
        }
    }

    /// <summary>
    /// 配置的讀取與基本驗證
    /// </summary>
    private static DatabaseInstance GetValidConnection(IConfiguration configuration)
    {
        var connections = configuration.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>();

        if (connections == null || !connections.Any())
        {
            throw new InvalidOperationException("無法找到 'DatabaseConnections' 設定，請檢查 appsettings.json。");
        }

        var conn = connections.First();

        return string.IsNullOrWhiteSpace(conn.ConnectionString)
            ? throw new ArgumentException($"連線字串 '{conn.Type}' 不能為空。")
            : conn;
    }

    /// <summary>
    /// Provider 的映射與 DI 註冊
    /// </summary>
    private static IServiceCollection RegisterDatabaseProvider<TContext>(
        this IServiceCollection services,
        DatabaseInstance connection)
        where TContext : DbContext
    {
        var type = connection.Type.ToUpperInvariant();
        
        return type switch
        {
            "NPGSQL" => services.AddNpgsqlServerPool<TContext>(connection.ConnectionString),
            _ => throw new NotSupportedException($"目前不支援資料庫類型: {connection.Type}")
        };
    }
}
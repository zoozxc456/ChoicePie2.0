using System.Reflection;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.AppSettings;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Outbox;
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
        /// <summary>
        /// UnitOfWork 連同 Outbox 一起註冊：UnitOfWork 寫 outbox，processor 派送。
        /// <paramref name="reliableHandlerAssemblies"/> 是放 <c>ReliableDomainEventNotification</c> handler 的組件，
        /// 用來決定哪些事件要進 outbox。需要宿主另外註冊 Data Protection（多台時金鑰必須共用）。
        /// </summary>
        public IServiceCollection AddSharedPersistence<TContext>(params Assembly[] reliableHandlerAssemblies)
            where TContext : DbContext
        {
            services.AddScoped<IUnitOfWork, EfUnitOfWork<TContext>>();

            services.AddSingleton(OutboxEventRegistry.FromAssemblies(reliableHandlerAssemblies));
            services.AddSingleton<OutboxSerializer>();
            services.AddSingleton<OutboxMessageFactory>();
            services.AddSingleton<OutboxDispatcher>();
            services.AddScoped(sp => new OutboxStore(sp.GetRequiredService<TContext>(), sp.GetRequiredService<TimeProvider>()));
            services.AddSingleton<OutboxProcessor>();
            services.AddHostedService(sp => sp.GetRequiredService<OutboxProcessor>());
            return services;
        }

        public IServiceCollection AddSharedDbContextPool<TContext>(IConfiguration configuration)
            where TContext : DbContext
        {
            var type = GetValidConnectionType(configuration);
            RegisterDatabaseProvider<TContext>(services, type);

            return services;
        }
    }

    /// <summary>
    /// 只在註冊當下驗證「有沒有設定、Provider 類型是什麼」，實際連線字串留給
    /// <see cref="DbContextExtensions.AddNpgsqlServerPool{TContext}"/> 在 DbContextOptions 建構當下才讀取。
    /// </summary>
    private static string GetValidConnectionType(IConfiguration configuration)
    {
        var connections = configuration.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>();

        if (connections == null || !connections.Any())
        {
            throw new InvalidOperationException("無法找到 'DatabaseConnections' 設定，請檢查 appsettings.json。");
        }

        return connections.First().Type.ToUpperInvariant();
    }

    /// <summary>
    /// Provider 的映射與 DI 註冊
    /// </summary>
    private static IServiceCollection RegisterDatabaseProvider<TContext>(
        this IServiceCollection services,
        string type)
        where TContext : DbContext
    {
        return type switch
        {
            "NPGSQL" => services.AddNpgsqlServerPool<TContext>(),
            _ => throw new NotSupportedException($"目前不支援資料庫類型: {type}")
        };
    }
}
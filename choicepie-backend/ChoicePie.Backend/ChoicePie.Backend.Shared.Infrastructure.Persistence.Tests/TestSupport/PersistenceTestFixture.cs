using Microsoft.Extensions.Logging.Abstractions;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Outbox;
using Microsoft.AspNetCore.DataProtection;
using Testcontainers.PostgreSql;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests.TestSupport;

/// <summary>
/// 每個測試檔案各自的 [OneTimeSetUp]/[OneTimeTearDown] 共用這個 helper 啟動一個 Testcontainers Postgres，
/// 用 EnsureCreatedAsync 建一個只有 TestWidget 這張表的最小 schema（不需要跑完整 product migrations）。
/// </summary>
public sealed class PersistenceTestFixture : IAsyncDisposable
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("choicepie_persistence_test")
        .WithUsername("choicepie")
        .WithPassword("choicepie")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
    }

    public TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .Options;

        return new TestDbContext(options);
    }

    public OutboxSerializer OutboxSerializer { get; } = new(new EphemeralDataProtectionProvider());

    public OutboxEventRegistry OutboxEventRegistry { get; } =
        OutboxEventRegistry.FromAssemblies(typeof(PersistenceTestFixture).Assembly);

    public EfUnitOfWork<TestDbContext> CreateUnitOfWork(TestDbContext context, MediatR.IPublisher publisher) =>
        new(context, publisher, new OutboxMessageFactory(OutboxSerializer, OutboxEventRegistry),
            NullLogger<EfUnitOfWork<TestDbContext>>.Instance);

    public async ValueTask DisposeAsync() => await _postgres.DisposeAsync();
}

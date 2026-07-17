using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace ChoicePie.Backend.WebApi.Tests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("choicepie_test")
        .WithUsername("choicepie")
        .WithPassword("choicepie")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync());

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChoicePieDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseConnections:0:Type"] = "NPGSQL",
                ["DatabaseConnections:0:ConnectionString"] = _postgres.GetConnectionString(),
                ["RedisConnection:ConnectionString"] = _redis.GetConnectionString(),
                ["Jwt:SigningKey"] = "test-signing-key-used-only-for-integration-tests-0123456789"
            });
        });
    }
}

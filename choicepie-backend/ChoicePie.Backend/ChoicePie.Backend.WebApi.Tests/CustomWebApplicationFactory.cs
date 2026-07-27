using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace ChoicePie.Backend.WebApi.Tests;

// 測試環境沒有真正的 SMTP server（appsettings.json 的 Smtp:Host 是空字串），SmtpEmailSender
// 會在 ConnectAsync 直接拋出例外；註冊/忘記密碼等流程會 raise domain event 觸發寄信，
// 若不替換掉真正的寄信實作，這些端點在測試中一律 500。
public sealed class NoOpEmailSender : IEmailSender
{
    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

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
                ["Jwt:SigningKey"] = "test-signing-key-used-only-for-integration-tests-0123456789",
                // appsettings.Development.json 設了 Auth:CookieDomain=minjie.demo（給共用 dev 環境用），
                // 但這裡的 HttpClient BaseAddress 是 localhost，瀏覽器/CookieContainer 不允許把跨網域
                // 的 Domain cookie 套用到 localhost，會直接拋 CookieException。覆寫成空字串，
                // 讓 AuthCookieExtensions 走「純本機」分支（不設 Domain、SameSite=Lax）。
                ["Auth:CookieDomain"] = ""
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IEmailSender>();
            services.AddScoped<IEmailSender, NoOpEmailSender>();
        });
    }
}

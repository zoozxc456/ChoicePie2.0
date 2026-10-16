using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Infrastructure.ExternalServices.Quizzes.Providers;
using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
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

// 真正的 Anthropic provider 會打外部付費 API：本機 user-secrets 有 ApiKey 時會真的呼叫，CI 沒有 key 則必定失敗。
// 整合測試只驗證端點流程，改用固定回傳指定題數的假 provider（沿用設定中的 Provider 名稱 "Anthropic"）。
public sealed class FakeQuizGenerationProvider : IQuizGenerationProvider
{
    public string ProviderName => "Anthropic";

    public Task<GeneratedQuestionsResult> GenerateAsync(
        string content, int questionCount, Difficulty difficulty, CancellationToken cancellationToken,
        WebPageSource? source = null)
    {
        var questions = Enumerable.Range(1, questionCount)
            .Select(i => new GeneratedQuestion($"Question {i}", ["A", "B", "C", "D"], 0, $"Explanation {i}"))
            .ToList();
        return Task.FromResult(new GeneratedQuestionsResult(questions, TokensUsed: 0));
    }
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
        // 用 UseSetting 而非 ConfigureAppConfiguration：minimal hosting 下 ConfigureAppConfiguration 要到
        // Build() 才套用，但 Program.cs 在那之前就會讀 builder.Configuration（例如 AddSharedDbContextPool
        // 檢查 DatabaseConnections）。本機有 user-secrets 補上所以看不出來，CI 沒有就會直接啟動失敗。
        // UseSetting 會在進入 Program 時就以 host 設定帶入，註冊階段即可讀到。
        builder.UseSetting("DatabaseConnections:0:Type", "NPGSQL");
        builder.UseSetting("DatabaseConnections:0:ConnectionString", _postgres.GetConnectionString());
        builder.UseSetting("RedisConnection:ConnectionString", _redis.GetConnectionString());
        builder.UseSetting("Jwt:SigningKey", "test-signing-key-used-only-for-integration-tests-0123456789");
        // appsettings.Development.json 設了 Auth:CookieDomain=minjie.demo（給共用 dev 環境用），
        // 但這裡的 HttpClient BaseAddress 是 localhost，瀏覽器/CookieContainer 不允許把跨網域
        // 的 Domain cookie 套用到 localhost，會直接拋 CookieException。覆寫成空字串，
        // 讓 AuthCookieExtensions 走「純本機」分支（不設 Domain、SameSite=Lax）。
        builder.UseSetting("Auth:CookieDomain", "");
        // 所有測試請求都來自同一個 IP／少數幾個帳號，正式環境的速率限制（例如 auth 每分鐘 10 次）
        // 會讓後面的測試全部吃到 429，整合測試不驗證限流本身，故全部放寬。
        foreach (var policy in new[] { "Auth", "Email", "Ai", "Write", "Global" })
        {
            builder.UseSetting($"RateLimiting:{policy}:PermitLimit", "100000");
        }

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IEmailSender>();
            services.AddScoped<IEmailSender, NoOpEmailSender>();
            services.RemoveAll<IQuizGenerationProvider>();
            services.AddScoped<IQuizGenerationProvider, FakeQuizGenerationProvider>();
        });
    }
}

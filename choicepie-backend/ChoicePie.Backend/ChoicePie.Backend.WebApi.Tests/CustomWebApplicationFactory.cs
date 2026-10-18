using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Infrastructure.ExternalServices.Quizzes.Providers;
using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Outbox;
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
// 會在 ConnectAsync 直接拋出例外；寄信由 outbox 派送，不換掉的話每封信都會一路重試到 dead letter。
// 改成把信收下來，讓測試能驗證信真的寄出、內容帶著正確的連結。
public sealed class CapturingEmailSender : IEmailSender
{
    private readonly System.Collections.Concurrent.ConcurrentQueue<(string To, string Subject, string HtmlBody)> _sent = new();

    public IReadOnlyCollection<(string To, string Subject, string HtmlBody)> Sent => _sent;

    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        _sent.Enqueue((toEmail, subject, htmlBody));
        return Task.CompletedTask;
    }
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
    // 只有自己建立的容器才由這個 factory 啟動與釋放；共用模式下由呼叫端負責。
    private readonly PostgreSqlContainer? _ownedPostgres;
    private readonly RedisContainer? _ownedRedis;
    private readonly Func<string> _postgresConnectionString;
    private readonly Func<string> _redisConnectionString;

    public CustomWebApplicationFactory()
    {
        var postgres = CreatePostgresContainer();
        var redis = CreateRedisContainer();
        _ownedPostgres = postgres;
        _ownedRedis = redis;
        _postgresConnectionString = postgres.GetConnectionString;
        _redisConnectionString = redis.GetConnectionString;
    }

    /// <summary>多節點測試：多個 factory 共用同一組 Postgres / Redis，每個 factory 就是一台獨立的後端。</summary>
    public CustomWebApplicationFactory(PostgreSqlContainer sharedPostgres, RedisContainer sharedRedis)
    {
        _postgresConnectionString = sharedPostgres.GetConnectionString;
        _redisConnectionString = sharedRedis.GetConnectionString;
    }

    public static PostgreSqlContainer CreatePostgresContainer() => new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("choicepie_test")
        .WithUsername("choicepie")
        .WithPassword("choicepie")
        .Build();

    public static RedisContainer CreateRedisContainer() => new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    public CapturingEmailSender Emails { get; } = new();

    public async Task InitializeAsync()
    {
        if (_ownedPostgres is not null && _ownedRedis is not null)
        {
            await Task.WhenAll(_ownedPostgres.StartAsync(), _ownedRedis.StartAsync());
        }

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChoicePieDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    /// <summary>
    /// 可靠事件（挑戰統計、封存題庫、寄信）由 outbox processor 在 commit 後幾秒內非同步派送；
    /// 斷言它們的結果之前先等 outbox 清空。
    /// </summary>
    public async Task WaitForOutboxDrainedAsync()
    {
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (true)
        {
            using (var scope = Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ChoicePieDbContext>();
                if (!await dbContext.Set<OutboxMessage>().AnyAsync(m => m.DeadLetteredAt == null))
                {
                    return;
                }
            }

            if (DateTime.UtcNow > deadline)
            {
                Assert.Fail("outbox 在 15 秒內沒有清空");
            }

            await Task.Delay(100);
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        if (_ownedPostgres is not null)
        {
            await _ownedPostgres.DisposeAsync();
        }

        if (_ownedRedis is not null)
        {
            await _ownedRedis.DisposeAsync();
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // 用 UseSetting 而非 ConfigureAppConfiguration：minimal hosting 下 ConfigureAppConfiguration 要到
        // Build() 才套用，但 Program.cs 在那之前就會讀 builder.Configuration（例如 AddSharedDbContextPool
        // 檢查 DatabaseConnections）。本機有 user-secrets 補上所以看不出來，CI 沒有就會直接啟動失敗。
        // UseSetting 會在進入 Program 時就以 host 設定帶入，註冊階段即可讀到。
        builder.UseSetting("DatabaseConnections:0:Type", "NPGSQL");
        builder.UseSetting("DatabaseConnections:0:ConnectionString", _postgresConnectionString());
        builder.UseSetting("RedisConnection:ConnectionString", _redisConnectionString());
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
            services.AddSingleton<IEmailSender>(Emails);
            services.RemoveAll<IQuizGenerationProvider>();
            services.AddScoped<IQuizGenerationProvider, FakeQuizGenerationProvider>();
        });
    }
}

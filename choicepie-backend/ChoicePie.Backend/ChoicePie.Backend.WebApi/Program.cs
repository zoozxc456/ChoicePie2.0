using ChoicePie.Backend.Application.GameRooms;
using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Infrastructure.Caching.Quizzes;
using ChoicePie.Backend.Infrastructure.ExternalServices.Quizzes.Providers;
using ChoicePie.Backend.Infrastructure.ExternalServices.WebPages;
using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Hosting.Extensions;
using ChoicePie.Backend.Shared.Hosting.HealthChecks;
using ChoicePie.Backend.Shared.Hosting.Logging;
using ChoicePie.Backend.Shared.Hosting.Monitoring;
using ChoicePie.Backend.Shared.Hosting.RateLimiting;
using ChoicePie.Backend.Shared.Infrastructure.Caching.Extensions;
using ChoicePie.Backend.Shared.Infrastructure.Email;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Abstractions;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Extensions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using ChoicePie.Backend.WebApi.Extensions;
using ChoicePie.Backend.WebApi.Hubs;
using MediatR;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.StackExchangeRedis;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.StackExchangeRedis;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StackExchange.Redis;

// Host 建好之前（DI 註冊、migration、seeder）就要能記錄，啟動失敗才不會無聲無息。
Log.Logger = SerilogConfiguration.CreateBootstrapLogger();

// dev server 偶爾出現的「連不上、process 已重啟」現象，若源自未被任何 middleware
// 攔到的背景執行緒例外，只有這裡能記下真正的崩潰原因與時間點。
AppDomain.CurrentDomain.UnhandledException += (_, e) =>
    Log.Fatal(e.ExceptionObject as Exception,
        "Unhandled exception，process 即將終止 (IsTerminating={IsTerminating})", e.IsTerminating);

TaskScheduler.UnobservedTaskException += (_, e) =>
{
    Log.Error(e.Exception, "Unobserved task exception");
    e.SetObserved();
};

try
{
    var builder = WebApplication.CreateBuilder(args);

    // 未設定 Sentry:Dsn 時完全不註冊。
    builder.AddChoicePieSentry();
    builder.AddChoicePieSerilog();

    builder.Services.AddControllers();
    builder.Services.Configure<AdminBootstrapSettings>(
        builder.Configuration.GetSection(AdminBootstrapSettings.SectionName));
    builder.Services.Configure<SmtpSettings>(
        builder.Configuration.GetSection(SmtpSettings.SectionName));
    builder.Services.Configure<GoogleSettings>(
        builder.Configuration.GetSection(GoogleSettings.SectionName));
    builder.Services.Configure<AiQuizGenerationSettings>(
        builder.Configuration.GetSection(AiQuizGenerationSettings.SectionName));
    builder.Services.Configure<CloudflareR2Settings>(
        builder.Configuration.GetSection(CloudflareR2Settings.SectionName));

    builder.Services
        .AddEndpointsApiExplorer()
        .AddHttpContextAccessor()
        .AddOpenApiService()
        .AddGlobalExceptionHandlers()
        .AddChoicePieApiVersioning()
        .AddJwtAuthentication(builder.Configuration)
        .AddChoicePieAuthorization()
        .AddChoicePieCors(builder.Configuration)
        .AddChoicePieForwardedHeaders(builder.Configuration)
        .AddChoicePieRateLimiting(builder.Configuration)
        .AddChoicePieCaching(builder.Configuration)
        .AddSharedDbContextPool<ChoicePieDbContext>(builder.Configuration)
        // 可靠事件 handler 都在 Application 組件：只有它們訂閱的事件會寫進 outbox。
        .AddSharedPersistence<ChoicePieDbContext>(typeof(ChoicePie.Backend.Application.AssemblyReference).Assembly)
        .AddHttpClient<AnthropicQuizGenerationProvider>()
        .Services
        // 刻意不實作 DI marker interface：Scrutor 的掃描註冊會蓋掉 typed client，導致 SSRF handler 被繞過。
        .AddHttpClient<IWebPageContentFetcher, WebPageContentFetcher>()
        .ConfigurePrimaryHttpMessageHandler(() => SsrfSafeConnector.CreateHandler())
        .Services
        .AddDomain(typeof(ChoicePie.Backend.Domain.AssemblyReference).Assembly)
        .AddApplication(typeof(ChoicePie.Backend.Application.AssemblyReference).Assembly)
        .AddInfrastructure(typeof(ChoicePie.Backend.Infrastructure.AssemblyReference).Assembly)
        .AddInfrastructure(typeof(ChoicePie.Backend.Shared.Infrastructure.Security.AssemblyReference).Assembly)
        .AddInfrastructure(typeof(ChoicePie.Backend.Shared.Infrastructure.Email.AssemblyReference).Assembly)
        .AddInfrastructure(typeof(ChoicePie.Backend.Shared.Hosting.AssemblyReference).Assembly)
        // 快取 decorator 不走 marker interface 掃描，否則會被當成另一個 IQuizQueryService 實作註冊。
        .Decorate<IQuizQueryService, CachedQuizQueryService>()
        // 排在 AddApplication 的 ValidationBehaviour 之後：驗證不過的指令不必等房間鎖。
        .AddTransient(typeof(IPipelineBehavior<,>), typeof(GameRoomLockBehavior<,>))
        .AddSingleton<DomainExceptionHubFilter>();

    builder.Services.AddChoicePieHealthChecks()
        .AddDbContextCheck<ChoicePieDbContext>("postgres", tags: [HealthCheckExtensions.ReadyTag]);

    builder.Services.AddSignalR(options =>
    {
        options.AddFilter<DomainExceptionHubFilter>();

        // 開發環境經過 Nuxt devProxy 多轉一層，keepalive ping 的往返延遲比直連後端高，
        // 預設 15s/30s 在網路稍有抖動時就會誤判斷線，故加大寬限空間。
        if (builder.Environment.IsDevelopment())
        {
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
        }
    }).AddStackExchangeRedis();

    // 多台後端：Redis backplane 讓 Clients.Group(...) 也送達連在其他節點上的連線。沿用快取註冊的那條 Redis 連線。
    builder.Services.AddOptions<RedisOptions>()
        .Configure<IConnectionMultiplexer>((options, redis) =>
        {
            options.Configuration.ChannelPrefix = RedisChannel.Literal("ChoicePie:SignalR");
            options.ConnectionFactory = _ => Task.FromResult(redis);
        });

    // Outbox payload 以 Data Protection 加密；任何一台寫入的訊息都可能由另一台派送，金鑰必須共用，
    // 所以金鑰環放在 Redis（沿用快取那條連線）。
    builder.Services.AddDataProtection().SetApplicationName("ChoicePie");
    builder.Services.AddOptions<KeyManagementOptions>()
        .Configure<IConnectionMultiplexer>((options, redis) =>
            options.XmlRepository = new RedisXmlRepository(() => redis.GetDatabase(), "ChoicePie:DataProtection-Keys"));

    var app = builder.Build();

    using (var startupScope = app.Services.CreateScope())
    {
        var dbContext = startupScope.ServiceProvider.GetRequiredService<ChoicePieDbContext>();

        // 多台同時啟動時一次只讓一台跑 migration 與 seed；其他台等它做完再跑，
        // 這時 migration 已經是 no-op，seeder 也會看到資料已存在而略過，不會重複建立管理員。
        const long startupMigrationLockKey = 0x43686F6963655069; // "ChoicePi"
        await using (await dbContext.Database.AcquireAdvisoryLockAsync(startupMigrationLockKey))
        {
            await dbContext.Database.MigrateAsync();

            var seeders = startupScope.ServiceProvider.GetServices<IDataSeeder>().OrderBy(s => s.Order);
            foreach (var seeder in seeders)
            {
                await seeder.SeedAsync();
            }
        }
    }

    if (app.Environment.IsDevelopment())
    {
        app.AddScalarApplicationConfiguration();
    }

    // 必須最先執行：先依信任清單還原真實客戶端 IP，後面的速率限制才分得出誰是誰。
    app.UseForwardedHeaders();

    // 在 UseExceptionHandler 外層：例外已被轉成 500 才回到這裡，不會重複記錄例外本身。
    app.UseChoicePieRequestLogging();

    app.UseExceptionHandler();

    // 健康檢查探針通常走容器內的純 HTTP，不能被導向 HTTPS。
    app.UseWhen(
        context => !context.Request.Path.StartsWithSegments("/health"),
        branch => branch.UseHttpsRedirection());
    app.UseRouting();

    app.UseCors(CorsExtensions.PolicyName);

    app.UseAuthentication();
    app.UseAuthorization();

    // 放在認證之後，才能用 sub claim 當分區 key。
    app.UseRateLimiter();

    app.MapControllers();
    app.MapChoicePieHealthChecks();
    app.MapHub<GameHub>("/api/gamehub");

    app.Run();

    // 不掛在 ProcessExit：它觸發時 finally 已經 CloseAndFlush，訊息會寫進已關閉的 logger。
    Log.Information("Process 正常結束");
}
// WebApplicationFactory 在 Build() 時以 HostAbortedException 中止 Program，不是真的失敗。
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Host 啟動或執行期間意外終止");
    // 在重新拋出前先 flush：未處理例外不保證會跑到 finally。
    await Log.CloseAndFlushAsync();
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program;
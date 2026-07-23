using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Hosting.Extensions;
using ChoicePie.Backend.Shared.Infrastructure.Caching.Extensions;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Abstractions;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Extensions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using ChoicePie.Backend.WebApi.Extensions;
using ChoicePie.Backend.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<AdminBootstrapSettings>(
    builder.Configuration.GetSection(AdminBootstrapSettings.SectionName));

builder.Services
    .AddEndpointsApiExplorer()
    .AddHttpContextAccessor()
    .AddOpenApiService()
    .AddGlobalExceptionHandlers()
    .AddChoicePieApiVersioning()
    .AddJwtAuthentication(builder.Configuration)
    .AddChoicePieAuthorization()
    .AddChoicePieCors(builder.Configuration)
    .AddChoicePieCaching(builder.Configuration)
    .AddSharedDbContextPool<ChoicePieDbContext>(builder.Configuration)
    .AddSharedPersistence<ChoicePieDbContext>()
    .AddDomain(typeof(ChoicePie.Backend.Domain.AssemblyReference).Assembly)
    .AddApplication(typeof(ChoicePie.Backend.Application.AssemblyReference).Assembly)
    .AddInfrastructure(typeof(ChoicePie.Backend.Infrastructure.AssemblyReference).Assembly)
    .AddInfrastructure(typeof(ChoicePie.Backend.Shared.Infrastructure.Security.AssemblyReference).Assembly)
    .AddInfrastructure(typeof(ChoicePie.Backend.Shared.Hosting.AssemblyReference).Assembly)
    .AddSingleton<DomainExceptionHubFilter>();

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
});

var app = builder.Build();

using (var startupScope = app.Services.CreateScope())
{
    var dbContext = startupScope.ServiceProvider.GetRequiredService<ChoicePieDbContext>();
    await dbContext.Database.MigrateAsync();

    var seeders = startupScope.ServiceProvider.GetServices<IDataSeeder>().OrderBy(s => s.Order);
    foreach (var seeder in seeders)
    {
        await seeder.SeedAsync();
    }
}

// dev server 偶爾出現的「連不上、process 已重啟」現象，若源自未被任何 middleware
// 攔到的背景執行緒例外，只有這裡能記下真正的崩潰原因與時間點。
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();

AppDomain.CurrentDomain.UnhandledException += (_, e) =>
    startupLogger.LogCritical(e.ExceptionObject as Exception,
        "Unhandled exception，process 即將終止 (IsTerminating={IsTerminating})", e.IsTerminating);

TaskScheduler.UnobservedTaskException += (_, e) =>
{
    startupLogger.LogError(e.Exception, "Unobserved task exception");
    e.SetObserved();
};

AppDomain.CurrentDomain.ProcessExit += (_, _) =>
    startupLogger.LogInformation("Process 正常結束");

if (app.Environment.IsDevelopment())
{
    app.AddScalarApplicationConfiguration();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors(CorsExtensions.PolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/api/gamehub");

app.Run();

public partial class Program;
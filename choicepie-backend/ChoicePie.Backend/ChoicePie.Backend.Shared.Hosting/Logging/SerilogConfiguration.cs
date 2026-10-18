using System.Reflection;
using ChoicePie.Backend.Shared.Hosting.Monitoring;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Templates;

namespace ChoicePie.Backend.Shared.Hosting.Logging;

public static class SerilogConfiguration
{
    private const string TextTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj} {TraceId}{NewLine}{Exception}";

    // 不用 RenderedCompactJsonFormatter：它的 @m 會把字串參數加上引號，且 Information 時省略 @l，
    // 收集端還得自己補預設等級。這裡固定輸出 @l，@m 也是純文字。
    private static readonly ExpressionTemplate JsonTemplate = new("{ {@t, @l, @m, @x, @tr, @sp, ..@p} }\n");

    private static readonly string ApplicationVersion =
        Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "unknown";

    /// <summary>
    /// Host 建好之前（讀設定、DI 註冊、migration、seeder）用的 logger，只寫 console。
    /// 刻意不用 <c>CreateBootstrapLogger()</c>：ReloadableLogger 只能 freeze 一次，
    /// 同一個 process 起多個 host（多節點整合測試）時第二個會丟 "The logger is already frozen"。
    /// </summary>
    public static Logger CreateBootstrapLogger()
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                              ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                              ?? Environments.Production;

        return WriteToConsole(new LoggerConfiguration().MinimumLevel.Information(), DefaultFormat(environmentName))
            .CreateLogger();
    }

    extension(WebApplicationBuilder builder)
    {
        /// <summary>
        /// 以 Serilog 取代 Microsoft.Extensions.Logging 的 providers；應用程式碼照舊注入 <c>ILogger&lt;T&gt;</c>。
        /// 每個 host 各自建立 logger（<c>preserveStaticLogger</c>），不覆寫全域的 <see cref="Log.Logger"/>。
        /// 啟用 Sentry 時，Error 以上改由 Serilog 的 Sentry sink 上報，MEL 的 Sentry provider 不再收到任何 log。
        /// </summary>
        public WebApplicationBuilder AddChoicePieSerilog()
        {
            builder.Services.AddSerilog(
                (services, loggerConfiguration) =>
                {
                    SentryConfiguration.EnsureSdkInitialized(services, builder.Configuration);
                    Configure(loggerConfiguration, builder.Configuration, builder.Environment);
                },
                preserveStaticLogger: true);
            return builder;
        }
    }

    public static LoggerConfiguration Configure(
        LoggerConfiguration loggerConfiguration, IConfiguration configuration, IHostEnvironment environment)
    {
        loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", environment.ApplicationName)
            .Enrich.WithProperty("Environment", environment.EnvironmentName)
            .Enrich.WithProperty("MachineName", Environment.MachineName)
            .Enrich.WithProperty("Version", ApplicationVersion);

        WriteToConsole(loggerConfiguration, ResolveFormat(configuration, environment.EnvironmentName));

        if (SentryConfiguration.IsEnabled(configuration))
        {
            loggerConfiguration.WriteTo.Sentry(SentryConfiguration.ConfigureSerilogSink);
        }

        return loggerConfiguration;
    }

    public static LogOutputFormat ResolveFormat(IConfiguration configuration, string environmentName)
        => configuration.GetSection(LogOutputSettings.SectionName).Get<LogOutputSettings>()?.Format
           ?? DefaultFormat(environmentName);

    private static LogOutputFormat DefaultFormat(string environmentName)
        => environmentName == Environments.Development ? LogOutputFormat.Text : LogOutputFormat.Json;

    private static LoggerConfiguration WriteToConsole(LoggerConfiguration loggerConfiguration, LogOutputFormat format)
        => format == LogOutputFormat.Json
            ? loggerConfiguration.WriteTo.Console(JsonTemplate)
            : loggerConfiguration.WriteTo.Console(outputTemplate: TextTemplate);
}

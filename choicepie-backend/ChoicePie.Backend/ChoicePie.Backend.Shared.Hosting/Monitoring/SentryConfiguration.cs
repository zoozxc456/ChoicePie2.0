using System.ComponentModel.DataAnnotations;
using ChoicePie.Backend.Shared.Application.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using ChoicePie.Backend.Shared.Kernel.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentry;
using Sentry.AspNetCore;
using Sentry.Serilog;
using Serilog.Events;

namespace ChoicePie.Backend.Shared.Hosting.Monitoring;

public class SentrySettings : IAppSetting
{
    public static string SectionName => "Sentry";

    /// <summary>留空代表完全不啟用 Sentry（no-op）。</summary>
    public string Dsn { get; set; } = string.Empty;

    public double TracesSampleRate { get; set; }
}

public static class SentryConfiguration
{
    extension(WebApplicationBuilder builder)
    {
        /// <summary>
        /// 僅在設定了 <c>Sentry:Dsn</c> 時才註冊 Sentry。未註冊時不會有任何 middleware、logger provider
        /// 或背景傳送行為。啟用後，<c>DefaultExceptionHandler</c> / Hub filter 以 LogError 記錄的未預期例外
        /// 會經由 Serilog 的 Sentry sink（見 <see cref="ConfigureSerilogSink"/>）送出。
        /// </summary>
        public WebApplicationBuilder AddChoicePieSentry()
        {
            var settings = Read(builder.Configuration);

            if (string.IsNullOrWhiteSpace(settings.Dsn))
            {
                return builder;
            }

            builder.WebHost.UseSentry((SentryAspNetCoreOptions options) => Configure(options, settings, builder.Environment.EnvironmentName));
            return builder;
        }
    }

    public static bool IsEnabled(IConfiguration configuration)
        => !string.IsNullOrWhiteSpace(Read(configuration).Dsn);

    /// <summary>
    /// SDK 由 <c>UseSentry</c> 初始化（DSN、環境、<see cref="BeforeSend"/> 過濾都在那裡），sink 只負責把 log 轉成事件。
    /// </summary>
    public static void ConfigureSerilogSink(SentrySerilogOptions options)
    {
        options.InitializeSdk = false;
        options.MinimumEventLevel = LogEventLevel.Error;
        options.MinimumBreadcrumbLevel = LogEventLevel.Information;
    }

    /// <summary>
    /// Sentry.AspNetCore 在第一次解析 <see cref="IHub"/> 時才初始化 SDK。原本由 MEL 的 Sentry provider 在啟動時解析，
    /// 改用 Serilog 後沒人解析，SDK 要等到第一個 request 經過 SentryMiddleware 才啟用——
    /// 啟動期間與背景服務（例如 OutboxProcessor）的 LogError 會全部丟失。建立 Serilog logger 時先解析一次。
    /// </summary>
    public static void EnsureSdkInitialized(IServiceProvider services, IConfiguration configuration)
    {
        if (IsEnabled(configuration))
        {
            services.GetRequiredService<IHub>();
        }
    }

    private static SentrySettings Read(IConfiguration configuration)
        => configuration.GetSection(SentrySettings.SectionName).Get<SentrySettings>() ?? new SentrySettings();

    public static void Configure(SentryAspNetCoreOptions options, SentrySettings settings, string environmentName)
    {
        options.Dsn = settings.Dsn;
        options.Environment = environmentName;
        options.SendDefaultPii = false;
        options.TracesSampleRate = settings.TracesSampleRate;
        options.SetBeforeSend(BeforeSend);
    }

    public static SentryEvent? BeforeSend(SentryEvent sentryEvent, SentryHint hint)
        => ShouldReport(sentryEvent.Exception) ? sentryEvent : null;

    /// <summary>
    /// 與全域例外處理鏈（DomainException → BadRequest → Default）對齊：
    /// 會被轉成 4xx 的預期例外不上報，只有落到 Default（5xx）的未預期例外才上報。
    /// </summary>
    public static bool ShouldReport(Exception? exception)
        => exception switch
        {
            null => true,
            AggregateException aggregate => aggregate.InnerExceptions.Any(ShouldReport),
            DomainException => false,
            AggregateValidationException => false,
            ValidationException => false,
            InvalidOperationException => false,
            OperationCanceledException => false,
            _ => true
        };
}

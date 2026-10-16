using System.ComponentModel.DataAnnotations;
using ChoicePie.Backend.Shared.Application.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using ChoicePie.Backend.Shared.Kernel.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Sentry;
using Sentry.AspNetCore;

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
        /// 會透過 Sentry 的 logging 整合自動送出。
        /// </summary>
        public WebApplicationBuilder AddChoicePieSentry()
        {
            var settings = builder.Configuration.GetSection(SentrySettings.SectionName).Get<SentrySettings>()
                           ?? new SentrySettings();

            if (string.IsNullOrWhiteSpace(settings.Dsn))
            {
                return builder;
            }

            builder.WebHost.UseSentry((SentryAspNetCoreOptions options) => Configure(options, settings, builder.Environment.EnvironmentName));
            return builder;
        }
    }

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

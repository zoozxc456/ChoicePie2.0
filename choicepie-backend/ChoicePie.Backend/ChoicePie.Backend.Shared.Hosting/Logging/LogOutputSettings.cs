using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;

namespace ChoicePie.Backend.Shared.Hosting.Logging;

public enum LogOutputFormat
{
    /// <summary>人類可讀的單行文字，給本機開發看。</summary>
    Text,

    /// <summary>每筆一行 JSON（@t、@l、@m、@x、@tr、@sp 加上所有屬性），給 Alloy / Loki 收集。</summary>
    Json
}

public class LogOutputSettings : IAppSetting
{
    public static string SectionName => "LogOutput";

    /// <summary>留空時依環境決定：Development 用 <see cref="LogOutputFormat.Text"/>，其他用 <see cref="LogOutputFormat.Json"/>。</summary>
    public LogOutputFormat? Format { get; set; }
}

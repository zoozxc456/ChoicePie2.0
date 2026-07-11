using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;

namespace ChoicePie.Backend.Shared.Infrastructure.Caching.AppSettings;

public class RedisConnectionSetting : IAppSetting
{
    public static string SectionName => "RedisConnection";
    public required string ConnectionString { get; set; }
}
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.AppSettings;

public class DatabaseInstance
{
    public required string Type { get; set; }

    public required string ConnectionString { get; set; }
}

public class DatabaseSettings : List<DatabaseInstance>, IAppSetting
{
    public static string SectionName => "DatabaseConnections";
}
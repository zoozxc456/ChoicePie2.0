namespace ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;

public class CorsSettings : IAppSetting
{
    public static string SectionName => "Cors";

    public string[] AllowedOrigins { get; set; } = ["http://localhost:3000"];
}

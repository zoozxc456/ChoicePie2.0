namespace ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;

public class GoogleSettings : IAppSetting
{
    public static string SectionName => "Google";

    public required string ClientId { get; set; }
}

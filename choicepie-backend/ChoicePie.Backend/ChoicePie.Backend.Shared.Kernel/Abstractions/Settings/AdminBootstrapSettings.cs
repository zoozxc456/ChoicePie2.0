namespace ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;

public class AdminBootstrapSettings : IAppSetting
{
    public static string SectionName => "AdminBootstrap";

    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Password { get; set; }
}

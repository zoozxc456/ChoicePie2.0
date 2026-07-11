namespace ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;

public class JwtSettings : IAppSetting
{
    public static string SectionName => "Jwt";

    public required string SigningKey { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public int AccessTokenExpirationSeconds { get; set; } = 60;
}

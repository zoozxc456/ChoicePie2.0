using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;

namespace ChoicePie.Backend.Shared.Infrastructure.Email;

public class SmtpSettings : IAppSetting
{
    public static string SectionName => "Smtp";

    public required string Host { get; set; }
    public int Port { get; set; } = 587;
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string FromAddress { get; set; }
    public string FromName { get; set; } = "ChoicePie";
    public bool UseSsl { get; set; } = true;

    // 用來組出信件內的重設密碼/驗證連結，例如 https://choicepie.app
    public required string FrontendBaseUrl { get; set; }
}

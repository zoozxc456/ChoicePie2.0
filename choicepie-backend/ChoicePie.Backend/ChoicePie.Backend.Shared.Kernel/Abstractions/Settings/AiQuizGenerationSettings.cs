namespace ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;

public class AiQuizGenerationSettings : IAppSetting
{
    public static string SectionName => "AiQuizGeneration";

    public required string Provider { get; set; }

    public required string ApiKey { get; set; }

    public required string Model { get; set; }
}

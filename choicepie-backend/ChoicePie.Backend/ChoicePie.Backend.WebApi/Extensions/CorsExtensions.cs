using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;

namespace ChoicePie.Backend.WebApi.Extensions;

public static class CorsExtensions
{
    public const string PolicyName = "ChoicePieCors";

    extension(IServiceCollection services)
    {
        public IServiceCollection AddChoicePieCors(IConfiguration configuration)
        {
            var corsSettings = configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>()
                                ?? new CorsSettings();

            services.AddCors(options =>
            {
                options.AddPolicy(PolicyName, policy => policy
                    .WithOrigins(corsSettings.AllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
            });

            return services;
        }
    }
}

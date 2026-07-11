using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

namespace ChoicePie.Backend.Shared.Hosting.Extensions;

public static class ScalarConfigurationExtensions
{
    public static IServiceCollection AddOpenApiService(this IServiceCollection services)
    {
        services.AddOpenApi();
        return services;
    }

    public static void AddScalarApplicationConfiguration(this WebApplication application)
    {
        const string scalarEndpointPrefix = "/api-docs";

        application.MapOpenApi();
        application.MapScalarApiReference(scalarEndpointPrefix, options =>
        {
            options.WithTitle("ChoicePie API Documentation")
                .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.HttpClient);
        });
    }
}
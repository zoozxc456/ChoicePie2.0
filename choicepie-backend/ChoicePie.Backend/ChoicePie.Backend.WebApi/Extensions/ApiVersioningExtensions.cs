using Asp.Versioning;

namespace ChoicePie.Backend.WebApi.Extensions;

public static class ApiVersioningExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddChoicePieApiVersioning()
        {
            services.AddApiVersioning(options =>
                {
                    options.DefaultApiVersion = new ApiVersion(1.0);
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.ReportApiVersions = true;
                })
                .AddMvc()
                .AddApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                });

            return services;
        }
    }
}

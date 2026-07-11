using ChoicePie.Backend.Shared.Hosting.Exceptions.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace ChoicePie.Backend.Shared.Hosting.Extensions;

public static class ExceptionHandlerExtensions
{
    public static IServiceCollection AddGlobalExceptionHandlers(this IServiceCollection services)
    {
        services.AddExceptionHandler<DomainExceptionHandler>();
        services.AddExceptionHandler<BadRequestExceptionHandler>();
        services.AddExceptionHandler<DefaultExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}

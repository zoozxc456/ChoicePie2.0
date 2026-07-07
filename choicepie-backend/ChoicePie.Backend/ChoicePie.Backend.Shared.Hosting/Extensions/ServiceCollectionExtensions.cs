using System.Reflection;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using Microsoft.Extensions.DependencyInjection;

namespace ChoicePie.Backend.Shared.Hosting.Extensions;

public static class DependencyInjectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication(Assembly assembly)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
            services.ScanDependencies(assembly);
            return services;
        }

        public IServiceCollection AddInfrastructure(Assembly assembly)
        {
            services.ScanDependencies(assembly);
            return services;
        }

        public IServiceCollection AddDomain(Assembly assembly)
        {
            services.ScanDependencies(assembly);
            return services;
        }

        private void ScanDependencies(Assembly assembly)
        {
            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(c => c.AssignableTo<IScopedDependency>())
                .AsSelfWithInterfaces()
                .WithScopedLifetime()
                .AddClasses(c => c.AssignableTo<ITransientDependency>())
                .AsSelfWithInterfaces()
                .WithTransientLifetime()
                .AddClasses(c => c.AssignableTo<ISingletonDependency>())
                .AsSelfWithInterfaces()
                .WithSingletonLifetime());
        }
    }
}
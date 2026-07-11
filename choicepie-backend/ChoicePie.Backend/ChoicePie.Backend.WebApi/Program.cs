using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Hosting.Extensions;
using ChoicePie.Backend.Shared.Infrastructure.Caching.Extensions;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Extensions;
using ChoicePie.Backend.WebApi.Extensions;
using ChoicePie.Backend.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services
    .AddEndpointsApiExplorer()
    .AddHttpContextAccessor()
    .AddOpenApiService()
    .AddGlobalExceptionHandlers()
    .AddChoicePieApiVersioning()
    .AddJwtAuthentication(builder.Configuration)
    .AddChoicePieAuthorization()
    .AddChoicePieCors(builder.Configuration)
    .AddChoicePieCaching(builder.Configuration)
    .AddSharedDbContextPool<ChoicePieDbContext>(builder.Configuration)
    .AddSharedPersistence<ChoicePieDbContext>()
    .AddDomain(typeof(ChoicePie.Backend.Domain.AssemblyReference).Assembly)
    .AddApplication(typeof(ChoicePie.Backend.Application.AssemblyReference).Assembly)
    .AddInfrastructure(typeof(ChoicePie.Backend.Infrastructure.AssemblyReference).Assembly)
    .AddInfrastructure(typeof(ChoicePie.Backend.Shared.Infrastructure.Security.AssemblyReference).Assembly)
    .AddInfrastructure(typeof(ChoicePie.Backend.Shared.Hosting.AssemblyReference).Assembly)
    .AddSingleton<DomainExceptionHubFilter>();

builder.Services.AddSignalR(options => options.AddFilter<DomainExceptionHubFilter>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.AddScalarApplicationConfiguration();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors(CorsExtensions.PolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/api/gamehub");

app.Run();

public partial class Program;
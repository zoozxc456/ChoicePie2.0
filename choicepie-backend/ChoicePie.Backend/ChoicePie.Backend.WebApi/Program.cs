using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Hosting.Extensions;
using ChoicePie.Backend.Shared.Infrastructure.Caching.Extensions;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthorization();
builder.Services.AddOpenApiService();
builder.Services.AddGlobalExceptionHandlers();

builder.Services
    .AddApplication(typeof(ChoicePie.Backend.Application.AssemblyReference).Assembly)
    .AddDomain(typeof(ChoicePie.Backend.Domain.AssemblyReference).Assembly)
    .AddInfrastructure(typeof(ChoicePie.Backend.Infrastructure.AssemblyReference).Assembly);

builder.Services.AddChoicePieCaching(builder.Configuration);
builder.Services.AddSharedDbContextPool<ChoicePieDbContext>(builder.Configuration)
    .AddSharedPersistence<ChoicePieDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.AddScalarApplicationConfiguration();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
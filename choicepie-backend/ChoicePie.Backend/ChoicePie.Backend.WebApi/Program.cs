using System.Text;
using Asp.Versioning;
using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Hosting.Extensions;
using ChoicePie.Backend.Shared.Infrastructure.Caching.Extensions;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Extensions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using ChoicePie.Backend.Shared.Kernel.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddOpenApiService();
builder.Services.AddGlobalExceptionHandlers();

builder.Services.AddApiVersioning(options =>
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

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                   ?? throw new InvalidOperationException("Jwt settings are not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("MemberOnly", policy => policy.RequireClaim(JwtClaimValues.RoleClaimType, JwtClaimValues.MemberRole))
    .AddPolicy("AdminOnly", policy => policy.RequireClaim(JwtClaimValues.RoleClaimType, JwtClaimValues.AdminRole));

builder.Services
    .AddApplication(typeof(ChoicePie.Backend.Application.AssemblyReference).Assembly)
    .AddDomain(typeof(ChoicePie.Backend.Domain.AssemblyReference).Assembly)
    .AddInfrastructure(typeof(ChoicePie.Backend.Infrastructure.AssemblyReference).Assembly)
    .AddInfrastructure(typeof(ChoicePie.Backend.Shared.Infrastructure.Security.AssemblyReference).Assembly)
    .AddInfrastructure(typeof(ChoicePie.Backend.Shared.Hosting.Extensions.DependencyInjectionExtensions).Assembly);

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

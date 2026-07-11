using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ChoicePie.Backend.Infrastructure.Persistence.Contexts;

/// <summary>
/// 供 `dotnet ef` 設計階段工具使用，避免建置整個 WebApi Host（含所有 MediatR handler 的 DI 驗證）。
/// 連線字串直接讀取 WebApi 專案的 User Secrets（UserSecretsId 與 WebApi.csproj 一致）。
/// </summary>
public class ChoicePieDbContextFactory : IDesignTimeDbContextFactory<ChoicePieDbContext>
{
    private const string WebApiUserSecretsId = "a4254835-b771-4d89-b77e-276ae2f0b6d5";

    public ChoicePieDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(WebApiUserSecretsId)
            .Build();

        var connectionString = configuration["DatabaseConnections:0:ConnectionString"];

        var optionsBuilder = new DbContextOptionsBuilder<ChoicePieDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        optionsBuilder.UseSnakeCaseNamingConvention();

        return new ChoicePieDbContext(optionsBuilder.Options);
    }
}

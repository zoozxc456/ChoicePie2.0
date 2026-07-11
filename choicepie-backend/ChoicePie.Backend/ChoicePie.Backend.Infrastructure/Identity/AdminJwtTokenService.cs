using ChoicePie.Backend.Application.AdminUsers.Contracts;
using ChoicePie.Backend.Domain.Aggregates.AdminUser;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using ChoicePie.Backend.Shared.Kernel.Auth;
using Microsoft.Extensions.Options;

namespace ChoicePie.Backend.Infrastructure.Identity;

public sealed class AdminJwtTokenService(IOptions<JwtSettings> jwtSettings) : IAdminTokenService, IScopedDependency
{
    public string GenerateAccessToken(AdminUser adminUser) =>
        JwtTokenBuilder.Build(jwtSettings.Value, adminUser.Id, JwtClaimValues.AdminRole);
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using ChoicePie.Backend.Shared.Kernel.Auth;
using Microsoft.IdentityModel.Tokens;

namespace ChoicePie.Backend.Infrastructure.Identity;

internal static class JwtTokenBuilder
{
    public static string Build(JwtSettings settings, Guid subjectId, string role)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, subjectId.ToString()),
            new Claim(JwtClaimValues.RoleClaimType, role)
        };

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(settings.AccessTokenExpirationSeconds),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

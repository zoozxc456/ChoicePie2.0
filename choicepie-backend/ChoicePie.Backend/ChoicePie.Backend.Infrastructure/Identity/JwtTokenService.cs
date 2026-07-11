using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using ChoicePie.Backend.Shared.Kernel.Auth;
using Microsoft.Extensions.Options;

namespace ChoicePie.Backend.Infrastructure.Identity;

public sealed class JwtTokenService(IOptions<JwtSettings> jwtSettings) : ITokenService, IScopedDependency
{
    public string GenerateAccessToken(Member member) =>
        JwtTokenBuilder.Build(jwtSettings.Value, member.Id, JwtClaimValues.MemberRole);
}

using ChoicePie.Backend.Shared.Kernel.Auth;

namespace ChoicePie.Backend.WebApi.Extensions;

public static class AuthorizationPolicyExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddChoicePieAuthorization()
        {
            services.AddAuthorizationBuilder()
                .AddPolicy("MemberOnly",
                    policy => policy.RequireClaim(JwtClaimValues.RoleClaimType, JwtClaimValues.MemberRole))
                .AddPolicy("AdminOnly",
                    policy => policy.RequireClaim(JwtClaimValues.RoleClaimType, JwtClaimValues.AdminRole));

            return services;
        }
    }
}

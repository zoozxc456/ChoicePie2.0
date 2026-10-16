using ChoicePie.Backend.Shared.Hosting.Authorization;
using ChoicePie.Backend.Shared.Kernel.Auth;
using Microsoft.AspNetCore.Authorization;

namespace ChoicePie.Backend.WebApi.Extensions;

public static class AuthorizationPolicyExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddChoicePieAuthorization()
        {
            services.AddAuthorizationBuilder()
                // MemberOnly 除了檢查 role claim，還會確認會員仍存在、未刪除、未停權（已簽發的 token 不能繼續使用）。
                .AddPolicy("MemberOnly",
                    policy => policy
                        .RequireClaim(JwtClaimValues.RoleClaimType, JwtClaimValues.MemberRole)
                        .AddRequirements(new ActiveMemberRequirement()))
                .AddPolicy("AdminOnly",
                    policy => policy.RequireClaim(JwtClaimValues.RoleClaimType, JwtClaimValues.AdminRole));

            services.AddScoped<IAuthorizationHandler, ActiveMemberAuthorizationHandler>();
            services.AddSingleton<IAuthorizationMiddlewareResultHandler, ActiveMemberAuthorizationResultHandler>();

            return services;
        }
    }
}

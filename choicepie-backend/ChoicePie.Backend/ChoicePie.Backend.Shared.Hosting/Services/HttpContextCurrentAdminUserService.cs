using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using ChoicePie.Backend.Shared.Kernel.Auth;
using Microsoft.AspNetCore.Http;

namespace ChoicePie.Backend.Shared.Hosting.Services;

public class HttpContextCurrentAdminUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentAdminUserService, IScopedDependency
{
    private const string SubjectClaimType = "sub";

    public Guid? AdminUserId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user?.FindFirst(JwtClaimValues.RoleClaimType)?.Value != JwtClaimValues.AdminRole)
            {
                return null;
            }

            var value = user.FindFirst(SubjectClaimType)?.Value;
            return Guid.TryParse(value, out var adminUserId) ? adminUserId : null;
        }
    }
}

using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using Microsoft.AspNetCore.Http;

namespace ChoicePie.Backend.Shared.Hosting.Services;

public class HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService, IScopedDependency
{
    // "sub" is the standard JWT subject claim (JwtRegisteredClaimNames.Sub). Using the literal
    // string avoids pulling the JWT package into Shared.Hosting just for a claim-type constant.
    private const string SubjectClaimType = "sub";

    public Guid? UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirst(SubjectClaimType)?.Value;
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }
}

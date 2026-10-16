using System.Security.Claims;
using ChoicePie.Backend.Shared.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace ChoicePie.Backend.Shared.Hosting.Authorization;

/// <summary>要求 JWT 的 sub 對應的會員仍然存在、未被刪除且目前未被停權。</summary>
public sealed class ActiveMemberRequirement : IAuthorizationRequirement;

public sealed class ActiveMemberAuthorizationHandler(IMemberActivityChecker checker)
    : AuthorizationHandler<ActiveMemberRequirement>
{
    // 與 HttpContextCurrentUserService 相同：直接用字面值，避免為了常數引入 JWT 套件。
    private const string SubjectClaimType = "sub";

    /// <summary>失敗原因的識別碼，供 <see cref="ActiveMemberAuthorizationResultHandler"/> 轉成 401。</summary>
    public const string InactiveMemberFailureCode = "InactiveMember";

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, ActiveMemberRequirement requirement)
    {
        var sub = context.User.FindFirst(SubjectClaimType)?.Value;
        if (!Guid.TryParse(sub, out var memberId))
        {
            context.Fail(new AuthorizationFailureReason(this, InactiveMemberFailureCode));
            return;
        }

        var cancellationToken = (context.Resource as HttpContext)?.RequestAborted ?? CancellationToken.None;
        if (await checker.IsActiveAsync(memberId, cancellationToken))
        {
            context.Succeed(requirement);
            return;
        }

        context.Fail(new AuthorizationFailureReason(this, InactiveMemberFailureCode));
    }
}

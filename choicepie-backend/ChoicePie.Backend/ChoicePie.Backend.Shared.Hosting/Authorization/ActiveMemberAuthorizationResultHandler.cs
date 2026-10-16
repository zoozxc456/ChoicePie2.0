using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace ChoicePie.Backend.Shared.Hosting.Authorization;

/// <summary>
/// 會員已被刪除／停權時，預設的授權失敗結果是 403；這裡改成 401（challenge），
/// 讓前端既有的「401 → refresh → 重試」流程接手（refresh 也會被拒絕，使用者最終被登出）。
/// 其他授權失敗維持預設行為。
/// </summary>
public sealed class ActiveMemberAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _default = new();

    public Task HandleAsync(
        RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        var inactiveMember = authorizeResult.AuthorizationFailure?.FailureReasons.Any(r =>
            r.Message == ActiveMemberAuthorizationHandler.InactiveMemberFailureCode) == true;

        if (inactiveMember)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        return _default.HandleAsync(next, context, policy, authorizeResult);
    }
}

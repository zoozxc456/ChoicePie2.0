using Microsoft.AspNetCore.Http;

namespace ChoicePie.Backend.Shared.Hosting.RateLimiting;

/// <summary>
/// 決定速率限制的分區 key：已登入優先用 <c>sub</c>（會員或管理員），否則退回客戶端 IP。
/// IP 取自 <c>Connection.RemoteIpAddress</c>，ForwardedHeaders middleware 已依信任清單還原真實 IP。
/// </summary>
public static class RateLimitPartitionKeyResolver
{
    private const string SubjectClaimType = "sub";

    public static string Resolve(HttpContext context)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var sub = user.FindFirst(SubjectClaimType)?.Value;
            if (!string.IsNullOrEmpty(sub))
            {
                return $"user:{sub}";
            }
        }

        return ResolveIp(context);
    }

    public static string ResolveIp(HttpContext context)
        => $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
}

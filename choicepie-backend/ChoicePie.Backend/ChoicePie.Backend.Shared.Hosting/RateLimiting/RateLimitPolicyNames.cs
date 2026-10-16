namespace ChoicePie.Backend.Shared.Hosting.RateLimiting;

/// <summary>
/// 具名速率限制 policy，搭配 <c>[EnableRateLimiting(RateLimitPolicyNames.X)]</c> 使用。
/// </summary>
public static class RateLimitPolicyNames
{
    public const string Auth = "auth";
    public const string Email = "email";
    public const string Ai = "ai";
    public const string Write = "write";
}

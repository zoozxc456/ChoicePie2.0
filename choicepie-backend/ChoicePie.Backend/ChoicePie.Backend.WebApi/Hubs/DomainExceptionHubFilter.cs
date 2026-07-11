using ChoicePie.Backend.Shared.Kernel.Exceptions;
using Microsoft.AspNetCore.SignalR;

namespace ChoicePie.Backend.WebApi.Hubs;

/// <summary>
/// SignalR 預設會吞掉例外訊息、只回傳通用的 HubException，這裡比照 WebApi 的全域例外處理鏈，
/// 把 DomainException 轉成帶有 UserFriendlyMessage 的 HubException，讓前端能顯示有意義的錯誤。
/// </summary>
public sealed class DomainExceptionHubFilter : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object?>> next)
    {
        try
        {
            return await next(invocationContext);
        }
        catch (DomainException ex)
        {
            throw new HubException(ex.UserFriendlyMessage, ex);
        }
    }
}

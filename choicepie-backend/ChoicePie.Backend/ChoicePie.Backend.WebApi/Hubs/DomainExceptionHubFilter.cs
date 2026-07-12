using ChoicePie.Backend.Shared.Kernel.Exceptions;
using Microsoft.AspNetCore.SignalR;

namespace ChoicePie.Backend.WebApi.Hubs;

/// <summary>
/// SignalR 預設會吞掉例外訊息、只回傳通用的 HubException，這裡比照 WebApi 的全域例外處理鏈，
/// 把 DomainException 轉成帶有 UserFriendlyMessage 的 HubException，讓前端能顯示有意義的錯誤。
/// </summary>
public sealed class DomainExceptionHubFilter(ILogger<DomainExceptionHubFilter> logger) : IHubFilter
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
            logger.LogWarning(ex, "Hub method {HubMethod} 因 domain 例外中止，ConnectionId={ConnectionId}",
                invocationContext.HubMethodName, invocationContext.Context.ConnectionId);
            throw new HubException(ex.UserFriendlyMessage, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Hub method {HubMethod} 發生未預期例外，ConnectionId={ConnectionId}",
                invocationContext.HubMethodName, invocationContext.Context.ConnectionId);
            throw;
        }
    }
}

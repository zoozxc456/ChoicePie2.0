namespace ChoicePie.Backend.Application.Identity.Contracts;

/// <summary>會員「是否仍可使用」判斷結果的快取失效埠；會員被刪除、停權或解除停權時呼叫。</summary>
public interface IMemberActivityCache
{
    Task InvalidateAsync(Guid memberId, CancellationToken cancellationToken);
}

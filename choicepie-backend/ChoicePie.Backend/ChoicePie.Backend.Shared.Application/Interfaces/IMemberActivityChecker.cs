namespace ChoicePie.Backend.Shared.Application.Interfaces;

/// <summary>
/// 判斷會員目前是否仍可使用系統：存在、未被刪除（關閉帳號）且目前沒有被停權。
/// 供授權層在每次請求驗證 access token 之後再確認一次，避免已簽發的 token 在帳號被刪除／停權後仍可使用。
/// </summary>
public interface IMemberActivityChecker
{
    Task<bool> IsActiveAsync(Guid memberId, CancellationToken cancellationToken);
}

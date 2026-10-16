namespace ChoicePie.Backend.Application.Account.Dtos;

// 帳號設定頁需要的個人資料 + 安全總覽。GoogleLinked 只表示有連結，目前未儲存 Google email，因此不回傳。
public sealed record AccountDto(
    Guid Id,
    string Name,
    string Email,
    string? Avatar,
    bool IsVerified,
    bool HasPassword,
    bool GoogleLinked);

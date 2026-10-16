using System.ComponentModel.DataAnnotations;
using MediatR;

namespace ChoicePie.Backend.Application.Account.Commands;

// 已有密碼的帳號必須帶 CurrentPassword；純 Google 帳號則是「設定密碼」，不需要。
// CurrentRefreshToken 由 controller 從 X-Refresh-Token header 帶入，用來保留目前這個 session。
public sealed class ChangePasswordCommand : IRequest
{
    public string? CurrentPassword { get; init; }

    [Required] [MinLength(8)] public required string NewPassword { get; init; }

    public string? CurrentRefreshToken { get; init; }
}

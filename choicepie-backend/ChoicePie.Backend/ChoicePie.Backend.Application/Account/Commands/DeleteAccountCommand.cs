using MediatR;

namespace ChoicePie.Backend.Application.Account.Commands;

// 有密碼的帳號必須帶 Password；純第三方登入帳號必須帶 ConfirmEmail（與帳號 email 相同）。
public sealed class DeleteAccountCommand : IRequest
{
    public string? Password { get; init; }
    public string? ConfirmEmail { get; init; }
}

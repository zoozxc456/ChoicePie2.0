using ChoicePie.Backend.Application.Account.Commands;

namespace ChoicePie.Backend.WebApi.Requests.Account;

public sealed record ChangePasswordRequest(string? CurrentPassword, string NewPassword)
{
    public ChangePasswordCommand ToCommand(string? currentRefreshToken) => new()
    {
        CurrentPassword = CurrentPassword,
        NewPassword = NewPassword,
        CurrentRefreshToken = currentRefreshToken
    };
}

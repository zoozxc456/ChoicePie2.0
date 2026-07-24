using ChoicePie.Backend.Application.Identity.Commands;

namespace ChoicePie.Backend.WebApi.Requests.Auth;

public sealed record ResetPasswordRequest(string Token, string Password, string ConfirmPassword)
{
    public ResetPasswordCommand ToCommand() => new() { Token = Token, Password = Password, ConfirmPassword = ConfirmPassword };
}

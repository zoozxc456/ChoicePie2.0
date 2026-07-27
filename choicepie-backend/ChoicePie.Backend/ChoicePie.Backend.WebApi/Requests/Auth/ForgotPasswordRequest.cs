using ChoicePie.Backend.Application.Identity.Commands;

namespace ChoicePie.Backend.WebApi.Requests.Auth;

public sealed record ForgotPasswordRequest(string Email)
{
    public ForgotPasswordCommand ToCommand() => new() { Email = Email };
}

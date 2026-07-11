using ChoicePie.Backend.Application.Identity.Commands;

namespace ChoicePie.Backend.WebApi.Requests.Auth;

public sealed record LoginRequest(string Email, string Password)
{
    public LoginCommand ToCommand() => new() { Email = Email, Password = Password };
}

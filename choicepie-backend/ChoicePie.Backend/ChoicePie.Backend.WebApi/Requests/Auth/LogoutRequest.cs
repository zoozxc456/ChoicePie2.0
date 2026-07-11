using ChoicePie.Backend.Application.Identity.Commands;

namespace ChoicePie.Backend.WebApi.Requests.Auth;

public sealed record LogoutRequest(string RefreshToken)
{
    public LogoutCommand ToCommand() => new() { RefreshToken = RefreshToken };
}

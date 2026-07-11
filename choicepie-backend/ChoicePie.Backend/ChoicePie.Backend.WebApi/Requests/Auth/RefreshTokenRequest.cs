using ChoicePie.Backend.Application.Identity.Commands;

namespace ChoicePie.Backend.WebApi.Requests.Auth;

public sealed record RefreshTokenRequest(string RefreshToken)
{
    public RefreshTokenCommand ToCommand() => new() { RefreshToken = RefreshToken };
}

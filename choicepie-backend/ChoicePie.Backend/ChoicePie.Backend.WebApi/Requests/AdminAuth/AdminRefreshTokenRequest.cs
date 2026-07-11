using ChoicePie.Backend.Application.AdminUsers.Commands;

namespace ChoicePie.Backend.WebApi.Requests.AdminAuth;

public sealed record AdminRefreshTokenRequest(string RefreshToken)
{
    public AdminRefreshTokenCommand ToCommand() => new() { RefreshToken = RefreshToken };
}

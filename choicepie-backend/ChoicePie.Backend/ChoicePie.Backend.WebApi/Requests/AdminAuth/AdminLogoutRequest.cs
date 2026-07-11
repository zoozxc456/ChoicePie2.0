using ChoicePie.Backend.Application.AdminUsers.Commands;

namespace ChoicePie.Backend.WebApi.Requests.AdminAuth;

public sealed record AdminLogoutRequest(string RefreshToken)
{
    public AdminLogoutCommand ToCommand() => new() { RefreshToken = RefreshToken };
}

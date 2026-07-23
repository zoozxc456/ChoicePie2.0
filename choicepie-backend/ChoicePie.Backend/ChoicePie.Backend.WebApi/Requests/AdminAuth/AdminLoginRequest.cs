using ChoicePie.Backend.Application.AdminUsers.Commands;

namespace ChoicePie.Backend.WebApi.Requests.AdminAuth;

public sealed record AdminLoginRequest(string Email, string Password)
{
    public AdminLoginCommand ToCommand() => new() { Email = Email, Password = Password };
}

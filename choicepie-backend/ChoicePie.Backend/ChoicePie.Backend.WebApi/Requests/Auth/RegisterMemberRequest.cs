using ChoicePie.Backend.Application.Identity.Commands;

namespace ChoicePie.Backend.WebApi.Requests.Auth;

public sealed record RegisterMemberRequest(string Email, string Name, string Password, string ConfirmPassword)
{
    public RegisterMemberCommand ToCommand() => new()
    {
        Email = Email,
        Name = Name,
        Password = Password,
        ConfirmPassword = ConfirmPassword
    };
}

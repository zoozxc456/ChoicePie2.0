using ChoicePie.Backend.Application.Account.Commands;

namespace ChoicePie.Backend.WebApi.Requests.Account;

public sealed record UpdateProfileRequest(string Name)
{
    public UpdateProfileCommand ToCommand() => new() { Name = Name };
}

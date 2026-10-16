using ChoicePie.Backend.Application.Account.Commands;

namespace ChoicePie.Backend.WebApi.Requests.Account;

public sealed record DeleteAccountRequest(string? Password, string? ConfirmEmail)
{
    public DeleteAccountCommand ToCommand() => new() { Password = Password, ConfirmEmail = ConfirmEmail };
}

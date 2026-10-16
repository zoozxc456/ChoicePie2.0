using ChoicePie.Backend.Application.Account.Commands;

namespace ChoicePie.Backend.WebApi.Requests.Account;

public sealed record LinkGoogleRequest(string IdToken)
{
    public LinkGoogleCommand ToCommand() => new() { IdToken = IdToken };
}

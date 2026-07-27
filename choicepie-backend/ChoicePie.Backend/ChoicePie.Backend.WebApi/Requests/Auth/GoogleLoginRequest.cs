using ChoicePie.Backend.Application.Identity.Commands;

namespace ChoicePie.Backend.WebApi.Requests.Auth;

public sealed record GoogleLoginRequest(string IdToken)
{
    public GoogleLoginCommand ToCommand() => new() { IdToken = IdToken };
}

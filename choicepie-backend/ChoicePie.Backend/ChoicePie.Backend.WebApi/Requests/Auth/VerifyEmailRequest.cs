using ChoicePie.Backend.Application.Identity.Commands;

namespace ChoicePie.Backend.WebApi.Requests.Auth;

public sealed record VerifyEmailRequest(string Token)
{
    public VerifyEmailCommand ToCommand() => new() { Token = Token };
}

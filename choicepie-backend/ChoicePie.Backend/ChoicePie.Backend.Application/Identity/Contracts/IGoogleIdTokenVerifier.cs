namespace ChoicePie.Backend.Application.Identity.Contracts;

public sealed record GoogleIdentity(string ProviderUserId, string Email, string Name, string? Picture);

public interface IGoogleIdTokenVerifier
{
    Task<GoogleIdentity> VerifyAsync(string idToken, CancellationToken cancellationToken = default);
}

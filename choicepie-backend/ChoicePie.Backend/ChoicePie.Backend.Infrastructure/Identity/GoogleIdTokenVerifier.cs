using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace ChoicePie.Backend.Infrastructure.Identity;

public sealed class GoogleIdTokenVerifier(IOptions<GoogleSettings> googleSettings)
    : IGoogleIdTokenVerifier, IScopedDependency
{
    public async Task<GoogleIdentity> VerifyAsync(string idToken, CancellationToken cancellationToken = default)
    {
        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [googleSettings.Value.ClientId]
            });
        }
        catch (InvalidJwtException)
        {
            throw new InvalidGoogleTokenException();
        }

        return new GoogleIdentity(payload.Subject, payload.Email, payload.Name, payload.Picture);
    }
}

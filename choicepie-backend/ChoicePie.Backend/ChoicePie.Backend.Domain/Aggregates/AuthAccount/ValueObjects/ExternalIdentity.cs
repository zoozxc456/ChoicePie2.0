using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums;
using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.AuthAccount.ValueObjects;

public sealed record ExternalIdentity : ValueObject
{
    public LoginProvider Provider { get; }
    public string ProviderUserId { get; }

    private ExternalIdentity(LoginProvider provider, string providerUserId)
    {
        Provider = provider;
        ProviderUserId = providerUserId;
    }

    public static ExternalIdentity Create(LoginProvider provider, string providerUserId)
    {
        return new ExternalIdentity(provider, providerUserId);
    }
}

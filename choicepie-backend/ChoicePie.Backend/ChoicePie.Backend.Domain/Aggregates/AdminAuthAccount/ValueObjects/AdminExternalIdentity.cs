using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Enums;
using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.ValueObjects;

public sealed record AdminExternalIdentity : ValueObject
{
    public AdminLoginProvider Provider { get; }
    public string ProviderUserId { get; }

    private AdminExternalIdentity(AdminLoginProvider provider, string providerUserId)
    {
        Provider = provider;
        ProviderUserId = providerUserId;
    }

    public static AdminExternalIdentity Create(AdminLoginProvider provider, string providerUserId)
    {
        return new AdminExternalIdentity(provider, providerUserId);
    }
}

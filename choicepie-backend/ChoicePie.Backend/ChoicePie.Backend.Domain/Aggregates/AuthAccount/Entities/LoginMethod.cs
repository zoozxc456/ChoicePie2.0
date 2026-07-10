using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums;
using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.AuthAccount.Entities;

public sealed class LoginMethod : Entity<Guid>
{
    public LoginProvider Provider { get; private set; } = null!;
    public string? PasswordHash { get; private set; }
    public string? ProviderUserId { get; private set; }

    private LoginMethod()
    {
    }

    public static LoginMethod CreateOriginal(string passwordHash) => new()
    {
        Id = Guid.NewGuid(),
        Provider = LoginProvider.Original,
        PasswordHash = passwordHash
    };

    public static LoginMethod CreateExternal(LoginProvider provider, string providerUserId) => new()
    {
        Id = Guid.NewGuid(),
        Provider = provider,
        ProviderUserId = providerUserId
    };
}

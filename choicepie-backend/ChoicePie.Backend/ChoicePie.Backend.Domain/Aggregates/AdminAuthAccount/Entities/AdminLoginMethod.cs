using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Enums;
using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Entities;

public sealed class AdminLoginMethod : AuditableEntity<Guid>
{
    public AdminLoginProvider Provider { get; private set; } = null!;
    public string? PasswordHash { get; private set; }
    public string? Salt { get; private set; }
    public string? ProviderUserId { get; private set; }

    private AdminLoginMethod()
    {
    }

    public static AdminLoginMethod CreateOriginal(string passwordHash, string salt) => new()
    {
        Id = Guid.NewGuid(),
        Provider = AdminLoginProvider.Original,
        PasswordHash = passwordHash,
        Salt = salt
    };

    public static AdminLoginMethod CreateExternal(AdminLoginProvider provider, string providerUserId) => new()
    {
        Id = Guid.NewGuid(),
        Provider = provider,
        ProviderUserId = providerUserId
    };
}
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Enums;
using ChoicePie.Backend.Shared.Kernel.Primitives;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;

namespace ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Entities;

public sealed class AdminLoginMethod : AuditableEntity<Guid>
{
    public AdminLoginProvider Provider { get; private set; } = null!;
    public HashedPassword? Password { get; private set; }
    public string? ProviderUserId { get; private set; }

    private AdminLoginMethod()
    {
    }

    public static AdminLoginMethod CreateOriginal(HashedPassword password) => new()
    {
        Id = Guid.NewGuid(),
        Provider = AdminLoginProvider.Original,
        Password = password
    };

    public static AdminLoginMethod CreateExternal(AdminLoginProvider provider, string providerUserId) => new()
    {
        Id = Guid.NewGuid(),
        Provider = provider,
        ProviderUserId = providerUserId
    };
}

using System.ComponentModel.DataAnnotations.Schema;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Enums;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.ValueObjects;
using ChoicePie.Backend.Shared.Kernel.Primitives;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;

namespace ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Entities;

public sealed class AdminLoginMethod : AuditableEntity<Guid>
{
    public HashedPassword? Password { get; private set; }
    public AdminExternalIdentity? External { get; private set; }

    [NotMapped]
    public AdminLoginProvider Provider => External?.Provider ?? AdminLoginProvider.Original;

    [NotMapped]
    public string? ProviderUserId => External?.ProviderUserId;

    private AdminLoginMethod()
    {
    }

    public static AdminLoginMethod CreateOriginal(HashedPassword password) => new()
    {
        Id = Guid.NewGuid(),
        Password = password
    };

    public static AdminLoginMethod CreateExternal(AdminLoginProvider provider, string providerUserId) => new()
    {
        Id = Guid.NewGuid(),
        External = AdminExternalIdentity.Create(provider, providerUserId)
    };
}

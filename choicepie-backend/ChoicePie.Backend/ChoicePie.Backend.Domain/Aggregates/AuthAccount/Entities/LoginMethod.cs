using System.ComponentModel.DataAnnotations.Schema;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.ValueObjects;
using ChoicePie.Backend.Shared.Kernel.Primitives;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;

namespace ChoicePie.Backend.Domain.Aggregates.AuthAccount.Entities;

public sealed class LoginMethod : AuditableEntity<Guid>
{
    public HashedPassword? Password { get; private set; }
    public ExternalIdentity? External { get; private set; }

    [NotMapped]
    public LoginProvider Provider => External?.Provider ?? LoginProvider.Original;

    [NotMapped]
    public string? ProviderUserId => External?.ProviderUserId;

    private LoginMethod()
    {
    }

    public static LoginMethod CreateOriginal(HashedPassword password) => new()
    {
        Id = Guid.NewGuid(),
        Password = password
    };

    public static LoginMethod CreateExternal(LoginProvider provider, string providerUserId) => new()
    {
        Id = Guid.NewGuid(),
        External = ExternalIdentity.Create(provider, providerUserId)
    };
}

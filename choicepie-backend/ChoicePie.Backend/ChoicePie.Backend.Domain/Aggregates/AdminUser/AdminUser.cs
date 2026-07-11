using ChoicePie.Backend.Domain.Aggregates.AdminUser.Enums;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Events;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;

namespace ChoicePie.Backend.Domain.Aggregates.AdminUser;

public sealed class AdminUser : AggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;
    public AdminRole Role { get; private set; } = null!;

    private AdminUser()
    {
    }

    public static AdminUser Create(string name, AdminRole role)
    {
        var personName = PersonName.Create(name, n => new InvalidAdminUserNameException(n));

        var adminUser = new AdminUser
        {
            Id = Guid.NewGuid(),
            Name = personName.Value,
            Role = role
        };

        adminUser.SetCreated(adminUser.Id);
        adminUser.AddDomainEvent(new AdminUserCreatedDomainEvent(adminUser.Id, adminUser.Name, role.Name));

        return adminUser;
    }
}

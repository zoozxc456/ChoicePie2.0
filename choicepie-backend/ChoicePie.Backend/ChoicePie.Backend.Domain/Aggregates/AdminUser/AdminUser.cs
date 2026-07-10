using ChoicePie.Backend.Domain.Aggregates.AdminUser.Enums;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Events;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.AdminUser;

public sealed class AdminUser : AggregateRoot<Guid>
{
    private const int MinNameLength = 2;
    private const int MaxNameLength = 20;

    public string Name { get; private set; } = null!;
    public AdminRole Role { get; private set; } = null!;

    private AdminUser()
    {
    }

    public static AdminUser Create(string name, AdminRole role)
    {
        ValidateName(name);

        var adminUser = new AdminUser
        {
            Id = Guid.NewGuid(),
            Name = name,
            Role = role
        };

        adminUser.SetCreated(adminUser.Id);
        adminUser.AddDomainEvent(new AdminUserCreatedDomainEvent(adminUser.Id, adminUser.Name, role.Name));

        return adminUser;
    }

    private static void ValidateName(string name)
    {
        var trimmed = name.Trim();

        if (trimmed.Length is < MinNameLength or > MaxNameLength)
        {
            throw new InvalidAdminUserNameException(name);
        }
    }
}

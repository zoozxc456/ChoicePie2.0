using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AdminUser;

namespace ChoicePie.Backend.Application.AdminUsers.Dtos;

public sealed record AdminUserDto(Guid Id, string Email, string Name, string Role, bool IsVerified, DateTime CreatedAt)
{
    public static AdminUserDto FromDomain(AdminUser adminUser, AdminAuthAccount adminAuthAccount) =>
        new(adminUser.Id, adminAuthAccount.Email.Value, adminUser.Name, adminUser.Role.Name, adminAuthAccount.IsVerified, adminUser.CreatedAt);
}

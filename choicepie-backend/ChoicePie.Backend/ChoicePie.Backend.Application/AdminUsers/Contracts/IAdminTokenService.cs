using ChoicePie.Backend.Domain.Aggregates.AdminUser;

namespace ChoicePie.Backend.Application.AdminUsers.Contracts;

public interface IAdminTokenService
{
    string GenerateToken(AdminUser adminUser);
}

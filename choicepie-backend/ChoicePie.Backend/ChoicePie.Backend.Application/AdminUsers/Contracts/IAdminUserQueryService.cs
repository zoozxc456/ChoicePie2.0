using ChoicePie.Backend.Application.AdminUsers.Dtos;

namespace ChoicePie.Backend.Application.AdminUsers.Contracts;

public interface IAdminUserQueryService
{
    Task<AdminUserDto> GetByIdAsync(Guid adminUserId, CancellationToken cancellationToken);
}

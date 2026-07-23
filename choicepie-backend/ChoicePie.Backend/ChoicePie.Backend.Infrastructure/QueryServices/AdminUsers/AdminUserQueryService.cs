using ChoicePie.Backend.Application.AdminUsers.Contracts;
using ChoicePie.Backend.Application.AdminUsers.Dtos;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AdminUser;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;

namespace ChoicePie.Backend.Infrastructure.QueryServices.AdminUsers;

public sealed class AdminUserQueryService(IReadRepository readRepository) : IAdminUserQueryService, IScopedDependency
{
    public Task<AdminUserDto> GetByIdAsync(Guid adminUserId, CancellationToken cancellationToken)
    {
        var adminUser = readRepository.Query<AdminUser>()
            .Where(a => a.Id == adminUserId)
            .Select(a => new { a.Id, a.Name, RoleName = a.Role.Name, a.CreatedAt })
            .FirstOrDefault()
            ?? throw new AdminUserNotFoundException(adminUserId);

        var adminAuthAccount = readRepository.Query<AdminAuthAccount>()
            .Where(a => a.AdminUserId == adminUserId)
            .Select(a => new { Email = a.Email.Value, a.IsVerified })
            .FirstOrDefault()
            ?? throw new AdminAuthAccountNotFoundException(adminUserId);

        return Task.FromResult(new AdminUserDto(adminUser.Id, adminAuthAccount.Email, adminUser.Name, adminUser.RoleName, adminAuthAccount.IsVerified, adminUser.CreatedAt));
    }
}

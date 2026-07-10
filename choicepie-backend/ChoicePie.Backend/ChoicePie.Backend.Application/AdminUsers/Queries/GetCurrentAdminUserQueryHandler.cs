using ChoicePie.Backend.Application.AdminUsers.Dtos;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.AdminUser;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.AdminUsers.Queries;

public sealed class GetCurrentAdminUserQueryHandler(
    IAdminUserRepository adminUserRepository,
    IAdminAuthAccountRepository adminAuthAccountRepository,
    ICurrentAdminUserService currentAdminUserService)
    : IRequestHandler<GetCurrentAdminUserQuery, AdminUserDto>
{
    public async Task<AdminUserDto> Handle(GetCurrentAdminUserQuery request, CancellationToken cancellationToken)
    {
        var adminUserId = currentAdminUserService.AdminUserId ?? throw new UnauthenticatedException();
        var adminUser = await adminUserRepository.GetByIdAsync(adminUserId, cancellationToken)
                         ?? throw new AdminUserNotFoundException(adminUserId);
        var adminAuthAccount = await adminAuthAccountRepository.FirstOrDefaultAsync(new AdminAuthAccountByAdminUserIdSpecification(adminUserId), cancellationToken)
                                ?? throw new AdminAuthAccountNotFoundException(adminUserId);

        return AdminUserDto.FromDomain(adminUser, adminAuthAccount);
    }
}

using ChoicePie.Backend.Application.AdminUsers.Contracts;
using ChoicePie.Backend.Application.AdminUsers.Dtos;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.AdminUsers.Queries;

public sealed class GetCurrentAdminUserQueryHandler(
    IAdminUserQueryService adminUserQueryService,
    ICurrentAdminUserService currentAdminUserService)
    : IRequestHandler<GetCurrentAdminUserQuery, AdminUserDto>
{
    public Task<AdminUserDto> Handle(GetCurrentAdminUserQuery request, CancellationToken cancellationToken)
    {
        var adminUserId = currentAdminUserService.AdminUserId ?? throw new UnauthenticatedException();
        return adminUserQueryService.GetByIdAsync(adminUserId, cancellationToken);
    }
}

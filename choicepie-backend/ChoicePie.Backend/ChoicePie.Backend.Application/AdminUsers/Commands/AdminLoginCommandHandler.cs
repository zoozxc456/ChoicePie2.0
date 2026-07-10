using ChoicePie.Backend.Application.AdminUsers.Contracts;
using ChoicePie.Backend.Application.AdminUsers.Dtos;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.AdminUser;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using MediatR;

namespace ChoicePie.Backend.Application.AdminUsers.Commands;

public sealed class AdminLoginCommandHandler(
    IAdminAuthAccountRepository adminAuthAccountRepository,
    IAdminUserRepository adminUserRepository,
    IPasswordHasher passwordHasher,
    IAdminTokenService tokenService)
    : IRequestHandler<AdminLoginCommand, AdminLoginResultDto>
{
    public async Task<AdminLoginResultDto> Handle(AdminLoginCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var adminAuthAccount = await adminAuthAccountRepository.FirstOrDefaultAsync(new AdminAuthAccountByEmailSpecification(email), cancellationToken)
                                ?? throw new InvalidCredentialsException();

        if (adminAuthAccount.OriginalPasswordHash is not { } passwordHash || !passwordHasher.Verify(request.Password, passwordHash))
        {
            throw new InvalidCredentialsException();
        }

        var adminUser = await adminUserRepository.GetByIdAsync(adminAuthAccount.AdminUserId, cancellationToken)
                         ?? throw new AdminUserNotFoundException(adminAuthAccount.AdminUserId);

        var token = tokenService.GenerateToken(adminUser);

        return new AdminLoginResultDto(AdminUserDto.FromDomain(adminUser, adminAuthAccount), token);
    }
}

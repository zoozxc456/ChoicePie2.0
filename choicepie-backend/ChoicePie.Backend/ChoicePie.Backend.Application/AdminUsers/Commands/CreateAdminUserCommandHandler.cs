using ChoicePie.Backend.Application.AdminUsers.Dtos;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.AdminUser;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Enums;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using MediatR;

namespace ChoicePie.Backend.Application.AdminUsers.Commands;

public sealed class CreateAdminUserCommandHandler(
    IAdminUserRepository adminUserRepository,
    IAdminAuthAccountRepository adminAuthAccountRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    ICurrentAdminUserService currentAdminUserService)
    : IRequestHandler<CreateAdminUserCommand, AdminUserDto>
{
    public async Task<AdminUserDto> Handle(CreateAdminUserCommand request, CancellationToken cancellationToken)
    {
        _ = currentAdminUserService.AdminUserId ?? throw new UnauthenticatedException();

        var role = AdminRole.FromName(request.Role) ?? throw new InvalidAdminRoleException(request.Role);
        var email = Email.Create(request.Email);

        var alreadyRegistered =
            await adminAuthAccountRepository.ExistsAsync(new AdminAuthAccountByEmailSpecification(email),
                cancellationToken);
        if (alreadyRegistered)
        {
            throw new EmailAlreadyRegisteredException(email.Value);
        }

        var adminUser = AdminUser.Create(request.Name, role);
        var (passwordHash, salt) = passwordHasher.Hash(request.Password);
        var adminAuthAccount = AdminAuthAccount.Create(email, passwordHash, salt, adminUser.Id);

        await adminUserRepository.AddAsync(adminUser, cancellationToken);
        await adminAuthAccountRepository.AddAsync(adminAuthAccount, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AdminUserDto.FromDomain(adminUser, adminAuthAccount);
    }
}
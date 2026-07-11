using ChoicePie.Backend.Application.AdminUsers.Contracts;
using ChoicePie.Backend.Application.AdminUsers.Dtos;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.AdminUser;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Enums;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using MediatR;
using RefreshTokenAggregate = ChoicePie.Backend.Domain.Aggregates.RefreshToken.RefreshToken;

namespace ChoicePie.Backend.Application.AdminUsers.Commands;

public sealed class AdminLoginCommandHandler(
    IAdminAuthAccountRepository adminAuthAccountRepository,
    IAdminUserRepository adminUserRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IAdminTokenService tokenService,
    IRefreshTokenGenerator refreshTokenGenerator,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AdminLoginCommand, AdminLoginResultDto>
{
    public async Task<AdminLoginResultDto> Handle(AdminLoginCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var adminAuthAccount =
            await adminAuthAccountRepository.FirstOrDefaultAsync(new AdminAuthAccountByEmailSpecification(email),
                cancellationToken)
            ?? throw new InvalidCredentialsException();

        if (adminAuthAccount.OriginalPassword is not { } hashedPassword ||
            !passwordHasher.Verify(request.Password, hashedPassword))
        {
            throw new InvalidCredentialsException();
        }

        var adminUser = await adminUserRepository.GetByIdAsync(adminAuthAccount.AdminUserId, cancellationToken)
                        ?? throw new AdminUserNotFoundException(adminAuthAccount.AdminUserId);

        var accessToken = tokenService.GenerateAccessToken(adminUser);
        var (rawRefreshToken, refreshTokenHash) = refreshTokenGenerator.Generate();
        var refreshToken =
            RefreshTokenAggregate.Issue(adminUser.Id, RefreshTokenOwnerType.Admin, refreshTokenHash, DateTime.UtcNow);

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AdminLoginResultDto(AdminUserDto.FromDomain(adminUser, adminAuthAccount), accessToken,
            rawRefreshToken);
    }
}

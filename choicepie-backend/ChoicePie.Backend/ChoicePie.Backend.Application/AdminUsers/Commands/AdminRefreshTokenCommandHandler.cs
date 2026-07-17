using ChoicePie.Backend.Application.AdminUsers.Contracts;
using ChoicePie.Backend.Application.AdminUsers.Dtos;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.AdminUser;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Enums;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Specifications;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;
using RefreshTokenAggregate = ChoicePie.Backend.Domain.Aggregates.RefreshToken.RefreshToken;

namespace ChoicePie.Backend.Application.AdminUsers.Commands;

public sealed class AdminRefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IAdminAuthAccountRepository adminAuthAccountRepository,
    IAdminUserRepository adminUserRepository,
    IAdminTokenService tokenService,
    IRefreshTokenGenerator refreshTokenGenerator,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<AdminRefreshTokenCommand, AdminLoginResultDto>
{
    public async Task<AdminLoginResultDto> Handle(AdminRefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash = refreshTokenGenerator.Hash(request.RefreshToken);
        var existingToken =
            await refreshTokenRepository.FirstOrDefaultAsync(new RefreshTokenByTokenHashSpecification(tokenHash),
                cancellationToken)
            ?? throw new InvalidRefreshTokenException();

        if (!existingToken.IsActive || existingToken.OwnerType != RefreshTokenOwnerType.Admin)
        {
            throw new InvalidRefreshTokenException();
        }

        var adminAuthAccount = await adminAuthAccountRepository.FirstOrDefaultAsync(
                                    new AdminAuthAccountByAdminUserIdSpecification(existingToken.OwnerId),
                                    cancellationToken)
                                ?? throw new AdminAuthAccountNotFoundException(existingToken.OwnerId);
        var adminUser = await adminUserRepository.GetByIdAsync(existingToken.OwnerId, cancellationToken)
                        ?? throw new AdminUserNotFoundException(existingToken.OwnerId);

        var accessToken = tokenService.GenerateAccessToken(adminUser);
        var (rawRefreshToken, refreshTokenHash) = refreshTokenGenerator.Generate();
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var newRefreshToken =
            RefreshTokenAggregate.Issue(adminUser.Id, RefreshTokenOwnerType.Admin, refreshTokenHash, now);

        existingToken.Revoke(now, newRefreshToken.Id);

        await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AdminLoginResultDto(AdminUserDto.FromDomain(adminUser, adminAuthAccount), accessToken,
            rawRefreshToken);
    }
}

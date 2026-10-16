using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Enums;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Specifications;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.Account.Commands;

public sealed class ChangePasswordCommandHandler(
    IAuthAccountRepository authAccountRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IRefreshTokenGenerator refreshTokenGenerator,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var memberId = currentUserService.UserId ?? throw new UnauthenticatedException();
        var authAccount = await authAccountRepository.FirstOrDefaultAsync(
                              new AuthAccountByMemberIdSpecification(memberId), cancellationToken)
                          ?? throw new AuthAccountNotFoundException(memberId);

        if (authAccount.OriginalPassword is { } currentHash)
        {
            if (string.IsNullOrEmpty(request.CurrentPassword) ||
                !passwordHasher.Verify(request.CurrentPassword, currentHash))
            {
                throw new InvalidCurrentPasswordException();
            }

            authAccount.ChangePassword(passwordHasher.Hash(request.NewPassword));
        }
        else
        {
            authAccount.AddPassword(passwordHasher.Hash(request.NewPassword));
        }

        await RevokeOtherSessionsAsync(memberId, request.CurrentRefreshToken, cancellationToken);

        await authAccountRepository.UpdateAsync(authAccount, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // 密碼變更後，除了目前這個 session 的 refresh token，其他裝置/瀏覽器的 session 一律登出。
    // 若沒帶目前的 refresh token（無法辨識哪一顆是目前 session），就全部撤銷。
    private async Task RevokeOtherSessionsAsync(Guid memberId, string? currentRefreshToken, CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var currentHash = string.IsNullOrEmpty(currentRefreshToken)
            ? null
            : refreshTokenGenerator.Hash(currentRefreshToken);

        var activeTokens = await refreshTokenRepository.ListAsync(
            new ActiveRefreshTokensByOwnerSpecification(memberId, RefreshTokenOwnerType.Member, now), ct);

        foreach (var token in activeTokens.Where(t => t.TokenHash != currentHash))
        {
            token.Revoke(now);
            await refreshTokenRepository.UpdateAsync(token, ct);
        }
    }
}

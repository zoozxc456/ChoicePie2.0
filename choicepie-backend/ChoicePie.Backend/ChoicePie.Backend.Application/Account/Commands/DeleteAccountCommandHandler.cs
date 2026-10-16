using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Enums;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Specifications;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.Account.Commands;

public sealed class DeleteAccountCommandHandler(
    IMemberRepository memberRepository,
    IAuthAccountRepository authAccountRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<DeleteAccountCommand>
{
    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var memberId = currentUserService.UserId ?? throw new UnauthenticatedException();
        var member = await memberRepository.GetByIdAsync(memberId, cancellationToken)
                     ?? throw new MemberNotFoundException(memberId);
        var authAccount = await authAccountRepository.FirstOrDefaultAsync(
                              new AuthAccountByMemberIdSpecification(memberId), cancellationToken)
                          ?? throw new AuthAccountNotFoundException(memberId);

        EnsureDeletionConfirmed(request, authAccount);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var activeTokens = await refreshTokenRepository.ListAsync(
            new ActiveRefreshTokensByOwnerSpecification(memberId, RefreshTokenOwnerType.Member, now),
            cancellationToken);
        foreach (var token in activeTokens)
        {
            token.Revoke(now);
            await refreshTokenRepository.UpdateAsync(token, cancellationToken);
        }

        member.CloseAccount();
        authAccount.MarkDeleted(memberId);

        await memberRepository.UpdateAsync(member, cancellationToken);
        await authAccountRepository.UpdateAsync(authAccount, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private void EnsureDeletionConfirmed(DeleteAccountCommand request, AuthAccount authAccount)
    {
        if (authAccount.OriginalPassword is { } hashedPassword)
        {
            if (string.IsNullOrEmpty(request.Password) || !passwordHasher.Verify(request.Password, hashedPassword))
            {
                throw new InvalidCurrentPasswordException();
            }

            return;
        }

        var confirmEmail = request.ConfirmEmail?.Trim();
        if (string.IsNullOrEmpty(confirmEmail) ||
            !string.Equals(confirmEmail, authAccount.Email.Value, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidAccountDeletionConfirmationException();
        }
    }
}

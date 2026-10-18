using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.PasswordResetToken;
using ChoicePie.Backend.Domain.Aggregates.PasswordResetToken.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.PasswordResetToken.Specifications;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.Commands;

public sealed class ResetPasswordCommandHandler(
    IPasswordResetTokenRepository passwordResetTokenRepository,
    IAuthAccountRepository authAccountRepository,
    IRefreshTokenGenerator tokenGenerator,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = tokenGenerator.Hash(request.Token);
        var resetToken = await passwordResetTokenRepository.FirstOrDefaultAsync(
            new PasswordResetTokenByTokenHashSpecification(tokenHash), cancellationToken)
            ?? throw new InvalidPasswordResetTokenException();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        resetToken.EnsureUsable(utcNow);

        var authAccount = await authAccountRepository.GetByIdAsync(resetToken.AuthAccountId, cancellationToken)
                           ?? throw new InvalidPasswordResetTokenException();

        authAccount.ChangePassword(passwordHasher.Hash(request.Password));
        resetToken.MarkUsed(utcNow);

        await authAccountRepository.UpdateAsync(authAccount, cancellationToken);
        await passwordResetTokenRepository.UpdateAsync(resetToken, cancellationToken);
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            // 同一顆 token 同時被另一個請求用掉：一次性 token 只能成功一次。
            throw new InvalidPasswordResetTokenException();
        }
    }
}

using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken;
using ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken.Specifications;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.Commands;

public sealed class VerifyEmailCommandHandler(
    IEmailVerificationTokenRepository emailVerificationTokenRepository,
    IAuthAccountRepository authAccountRepository,
    IRefreshTokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<VerifyEmailCommand>
{
    public async Task Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = tokenGenerator.Hash(request.Token);
        var verificationToken = await emailVerificationTokenRepository.FirstOrDefaultAsync(
            new EmailVerificationTokenByTokenHashSpecification(tokenHash), cancellationToken)
            ?? throw new InvalidEmailVerificationTokenException();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        verificationToken.EnsureUsable(utcNow);

        var authAccount = await authAccountRepository.GetByIdAsync(verificationToken.AuthAccountId, cancellationToken)
                           ?? throw new InvalidEmailVerificationTokenException();

        authAccount.Verify();
        verificationToken.MarkUsed(utcNow);

        await authAccountRepository.UpdateAsync(authAccount, cancellationToken);
        await emailVerificationTokenRepository.UpdateAsync(verificationToken, cancellationToken);
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            // 同一顆 token 同時被另一個請求用掉：一次性 token 只能成功一次。
            throw new InvalidEmailVerificationTokenException();
        }
    }
}

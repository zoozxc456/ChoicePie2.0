using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.PasswordResetToken;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.Commands;

public sealed class ForgotPasswordCommandHandler(
    IAuthAccountRepository authAccountRepository,
    IPasswordResetTokenRepository passwordResetTokenRepository,
    IRefreshTokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var authAccount = await authAccountRepository.FirstOrDefaultAsync(
            new AuthAccountByEmailSpecification(email), cancellationToken);

        // 帳號不存在時靜默返回，避免透過此 API 探測哪些 email 已註冊。
        if (authAccount is null)
        {
            return;
        }

        var (rawToken, tokenHash) = tokenGenerator.Generate();
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var resetToken = PasswordResetToken.Issue(authAccount.Id, email.Value, rawToken, tokenHash, utcNow);

        await passwordResetTokenRepository.AddAsync(resetToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

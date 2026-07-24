using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.Commands;

public sealed class ResendVerificationEmailCommandHandler(
    IAuthAccountRepository authAccountRepository,
    IEmailVerificationTokenRepository emailVerificationTokenRepository,
    ICurrentUserService currentUserService,
    IRefreshTokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<ResendVerificationEmailCommand>
{
    public async Task Handle(ResendVerificationEmailCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthenticatedException();

        var authAccount = await authAccountRepository.FirstOrDefaultAsync(
            new AuthAccountByMemberIdSpecification(userId), cancellationToken)
            ?? throw new AuthAccountNotFoundException(userId);

        if (authAccount.IsVerified)
        {
            throw new EmailAlreadyVerifiedException();
        }

        var (rawToken, tokenHash) = tokenGenerator.Generate();
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var verificationToken = EmailVerificationToken.Issue(
            authAccount.Id, authAccount.Email.Value, rawToken, tokenHash, utcNow);

        await emailVerificationTokenRepository.AddAsync(verificationToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

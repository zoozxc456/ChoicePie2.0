using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.Account.Commands;

public sealed class LinkGoogleCommandHandler(
    IGoogleIdTokenVerifier googleIdTokenVerifier,
    IAuthAccountRepository authAccountRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LinkGoogleCommand>
{
    public async Task Handle(LinkGoogleCommand request, CancellationToken cancellationToken)
    {
        var memberId = currentUserService.UserId ?? throw new UnauthenticatedException();
        var authAccount = await authAccountRepository.FirstOrDefaultAsync(
                              new AuthAccountByMemberIdSpecification(memberId), cancellationToken)
                          ?? throw new AuthAccountNotFoundException(memberId);

        var googleIdentity = await googleIdTokenVerifier.VerifyAsync(request.IdToken, cancellationToken);

        var owner = await authAccountRepository.FirstOrDefaultAsync(
            new AuthAccountByExternalIdentitySpecification(LoginProvider.Google, googleIdentity.ProviderUserId),
            cancellationToken);
        if (owner is not null && owner.Id != authAccount.Id)
        {
            throw new ExternalIdentityAlreadyLinkedException(LoginProvider.Google);
        }

        authAccount.AddLoginMethod(LoginProvider.Google, googleIdentity.ProviderUserId);

        await authAccountRepository.UpdateAsync(authAccount, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

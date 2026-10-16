using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.Account.Commands;

public sealed class UnlinkGoogleCommandHandler(
    IAuthAccountRepository authAccountRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UnlinkGoogleCommand>
{
    public async Task Handle(UnlinkGoogleCommand request, CancellationToken cancellationToken)
    {
        var memberId = currentUserService.UserId ?? throw new UnauthenticatedException();
        var authAccount = await authAccountRepository.FirstOrDefaultAsync(
                              new AuthAccountByMemberIdSpecification(memberId), cancellationToken)
                          ?? throw new AuthAccountNotFoundException(memberId);

        authAccount.RemoveLoginMethod(LoginProvider.Google);

        await authAccountRepository.UpdateAsync(authAccount, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

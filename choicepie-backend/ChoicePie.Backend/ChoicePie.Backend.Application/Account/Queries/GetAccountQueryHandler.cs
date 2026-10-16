using ChoicePie.Backend.Application.Account.Contracts;
using ChoicePie.Backend.Application.Account.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.Account.Queries;

public sealed class GetAccountQueryHandler(
    IAccountQueryService accountQueryService,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetAccountQuery, AccountDto>
{
    public Task<AccountDto> Handle(GetAccountQuery request, CancellationToken cancellationToken)
    {
        var memberId = currentUserService.UserId ?? throw new UnauthenticatedException();
        return accountQueryService.GetByMemberIdAsync(memberId, cancellationToken);
    }
}

using ChoicePie.Backend.Application.Account.Contracts;
using ChoicePie.Backend.Application.Account.Dtos;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;

namespace ChoicePie.Backend.Infrastructure.QueryServices.Account;

public sealed class AccountQueryService(IReadRepository readRepository) : IAccountQueryService, IScopedDependency
{
    public Task<AccountDto> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken)
    {
        var member = readRepository.Query<Member>()
                         .Where(m => m.Id == memberId)
                         .Select(m => new { m.Id, m.Name, m.Avatar })
                         .FirstOrDefault()
                     ?? throw new MemberNotFoundException(memberId);

        var auth = readRepository.Query<AuthAccount>()
                       .Where(a => a.MemberId == memberId)
                       .Select(a => new
                       {
                           Email = a.Email.Value,
                           a.IsVerified,
                           HasPassword = a.LoginMethods.Any(l => l.External == null),
                           GoogleLinked = a.LoginMethods.Any(l =>
                               l.External != null && l.External.Provider == LoginProvider.Google)
                       })
                       .FirstOrDefault()
                   ?? throw new AuthAccountNotFoundException(memberId);

        return Task.FromResult(new AccountDto(
            member.Id, member.Name, auth.Email, member.Avatar, auth.IsVerified, auth.HasPassword, auth.GoogleLinked));
    }
}

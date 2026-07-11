using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Application.Identity.Dtos;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;

namespace ChoicePie.Backend.Infrastructure.QueryServices.Identity;

public sealed class MemberQueryService(IReadRepository readRepository) : IMemberQueryService, IScopedDependency
{
    public Task<MemberDto> GetByIdAsync(Guid memberId, CancellationToken cancellationToken)
    {
        var member = readRepository.Query<Member>()
            .Where(m => m.Id == memberId)
            .Select(m => new { m.Id, m.Name, m.Avatar, m.CreatedAt })
            .FirstOrDefault()
            ?? throw new MemberNotFoundException(memberId);

        var authAccount = readRepository.Query<AuthAccount>()
            .Where(a => a.MemberId == memberId)
            .Select(a => new { Email = a.Email.Value, a.IsVerified })
            .FirstOrDefault()
            ?? throw new AuthAccountNotFoundException(memberId);

        return Task.FromResult(new MemberDto(member.Id, authAccount.Email, member.Name, member.Avatar, authAccount.IsVerified, member.CreatedAt));
    }
}

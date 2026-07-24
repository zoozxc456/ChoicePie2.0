using ChoicePie.Backend.Application.AdminMembers.Dtos;
using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Application.Identity.Dtos;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Contracts;
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

    public Task<PagedResult<AdminMemberSummaryDto>> AdminListAsync(
        string? search, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = readRepository.Query<Member>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(m => m.Name.Contains(search));
        }

        var totalCount = query.Count();

        var joined =
            from m in query
            join a in readRepository.Query<AuthAccount>() on m.Id equals a.MemberId into authGroup
            from auth in authGroup.DefaultIfEmpty()
            orderby m.CreatedAt descending
            select new AdminMemberSummaryDto(
                m.Id,
                m.Name,
                auth != null ? auth.Email.Value : "Unknown",
                m.IsSuspended,
                m.SuspendedReason,
                m.SuspendedUntil,
                m.CreatedAt);

        var items = joined
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PagedResult<AdminMemberSummaryDto>(items, pageNumber, pageSize, totalCount));
    }
}

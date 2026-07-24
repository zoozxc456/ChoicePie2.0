using ChoicePie.Backend.Application.AdminMembers.Dtos;
using ChoicePie.Backend.Application.Identity.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;

namespace ChoicePie.Backend.Application.Identity.Contracts;

public interface IMemberQueryService
{
    Task<MemberDto> GetByIdAsync(Guid memberId, CancellationToken cancellationToken);

    Task<PagedResult<AdminMemberSummaryDto>> AdminListAsync(
        string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);
}

using ChoicePie.Backend.Application.Identity.Dtos;

namespace ChoicePie.Backend.Application.Identity.Contracts;

public interface IMemberQueryService
{
    Task<MemberDto> GetByIdAsync(Guid memberId, CancellationToken cancellationToken);
}

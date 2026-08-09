using ChoicePie.Backend.Application.MembershipTiers.Dtos;

namespace ChoicePie.Backend.Application.MembershipTiers.Contracts;

public interface IMembershipTierQueryService
{
    Task<IReadOnlyList<MembershipTierDto>> ListAsync(CancellationToken cancellationToken);

    Task<MembershipTierDto> GetByIdAsync(Guid tierId, CancellationToken cancellationToken);
}

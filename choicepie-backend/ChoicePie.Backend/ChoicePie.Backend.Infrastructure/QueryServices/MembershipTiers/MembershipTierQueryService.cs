using ChoicePie.Backend.Application.MembershipTiers.Contracts;
using ChoicePie.Backend.Application.MembershipTiers.Dtos;
using ChoicePie.Backend.Domain.Aggregates.MembershipTier.Exceptions;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using MembershipTierAggregate = ChoicePie.Backend.Domain.Aggregates.MembershipTier.MembershipTier;

namespace ChoicePie.Backend.Infrastructure.QueryServices.MembershipTiers;

public sealed class MembershipTierQueryService(IReadRepository readRepository) : IMembershipTierQueryService, IScopedDependency
{
    public Task<IReadOnlyList<MembershipTierDto>> ListAsync(CancellationToken cancellationToken)
    {
        var tiers = readRepository.Query<MembershipTierAggregate>()
            .OrderBy(t => t.DailyGenerationLimit)
            .Select(t => new MembershipTierDto(t.Id, t.Name, t.DailyGenerationLimit, t.DailyTokenBudget, t.IsDefault, t.CreatedAt))
            .ToList();

        return Task.FromResult<IReadOnlyList<MembershipTierDto>>(tiers);
    }

    public Task<MembershipTierDto> GetByIdAsync(Guid tierId, CancellationToken cancellationToken)
    {
        var tier = readRepository.Query<MembershipTierAggregate>()
            .Where(t => t.Id == tierId)
            .Select(t => new MembershipTierDto(t.Id, t.Name, t.DailyGenerationLimit, t.DailyTokenBudget, t.IsDefault, t.CreatedAt))
            .FirstOrDefault()
            ?? throw new MembershipTierNotFoundException(tierId);

        return Task.FromResult(tier);
    }
}

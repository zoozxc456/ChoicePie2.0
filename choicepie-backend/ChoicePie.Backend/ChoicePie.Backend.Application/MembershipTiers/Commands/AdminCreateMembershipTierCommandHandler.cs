using ChoicePie.Backend.Application.MembershipTiers.Dtos;
using ChoicePie.Backend.Domain.Aggregates.MembershipTier;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;
using MembershipTierAggregate = ChoicePie.Backend.Domain.Aggregates.MembershipTier.MembershipTier;

namespace ChoicePie.Backend.Application.MembershipTiers.Commands;

public sealed class AdminCreateMembershipTierCommandHandler(
    IMembershipTierRepository membershipTierRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AdminCreateMembershipTierCommand, MembershipTierDto>
{
    public async Task<MembershipTierDto> Handle(AdminCreateMembershipTierCommand request, CancellationToken cancellationToken)
    {
        var tier = MembershipTierAggregate.Create(request.Name, request.DailyGenerationLimit, request.DailyTokenBudget);

        await membershipTierRepository.AddAsync(tier, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new MembershipTierDto(
            tier.Id, tier.Name, tier.DailyGenerationLimit, tier.DailyTokenBudget, tier.IsDefault, tier.CreatedAt);
    }
}

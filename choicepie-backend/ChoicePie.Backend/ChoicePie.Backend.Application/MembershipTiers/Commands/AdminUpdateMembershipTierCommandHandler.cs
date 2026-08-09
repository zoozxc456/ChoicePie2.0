using ChoicePie.Backend.Application.MembershipTiers.Dtos;
using ChoicePie.Backend.Domain.Aggregates.MembershipTier;
using ChoicePie.Backend.Domain.Aggregates.MembershipTier.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.MembershipTiers.Commands;

public sealed class AdminUpdateMembershipTierCommandHandler(
    IMembershipTierRepository membershipTierRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AdminUpdateMembershipTierCommand, MembershipTierDto>
{
    public async Task<MembershipTierDto> Handle(AdminUpdateMembershipTierCommand request, CancellationToken cancellationToken)
    {
        var tier = await membershipTierRepository.GetByIdAsync(request.TierId, cancellationToken)
                   ?? throw new MembershipTierNotFoundException(request.TierId);

        tier.Update(request.Name, request.DailyGenerationLimit, request.DailyTokenBudget);

        await membershipTierRepository.UpdateAsync(tier, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new MembershipTierDto(
            tier.Id, tier.Name, tier.DailyGenerationLimit, tier.DailyTokenBudget, tier.IsDefault, tier.CreatedAt);
    }
}

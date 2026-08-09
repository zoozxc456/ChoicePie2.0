using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.MembershipTier;
using ChoicePie.Backend.Domain.Aggregates.MembershipTier.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.AdminMembers.Commands;

public sealed class AdminAssignMemberTierCommandHandler(
    IMemberRepository memberRepository,
    IMembershipTierRepository membershipTierRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AdminAssignMemberTierCommand>
{
    public async Task Handle(AdminAssignMemberTierCommand request, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(request.MemberId, cancellationToken)
                     ?? throw new MemberNotFoundException(request.MemberId);

        _ = await membershipTierRepository.GetByIdAsync(request.TierId, cancellationToken)
            ?? throw new MembershipTierNotFoundException(request.TierId);

        member.AssignTier(request.TierId);

        await memberRepository.UpdateAsync(member, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

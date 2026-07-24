using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.AdminMembers.Commands;

public sealed class AdminSuspendMemberCommandHandler(
    IMemberRepository memberRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AdminSuspendMemberCommand>
{
    public async Task Handle(AdminSuspendMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(request.MemberId, cancellationToken)
                     ?? throw new MemberNotFoundException(request.MemberId);

        member.Suspend(request.Reason, request.Until);

        await memberRepository.UpdateAsync(member, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.AdminMembers.Commands;

public sealed class AdminUnsuspendMemberCommandHandler(
    IMemberRepository memberRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AdminUnsuspendMemberCommand>
{
    public async Task Handle(AdminUnsuspendMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(request.MemberId, cancellationToken)
                     ?? throw new MemberNotFoundException(request.MemberId);

        member.Unsuspend();

        await memberRepository.UpdateAsync(member, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

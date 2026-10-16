using ChoicePie.Backend.Application.Account.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.Account.Commands;

public sealed class UpdateProfileCommandHandler(
    IMemberRepository memberRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProfileCommand, ProfileDto>
{
    public async Task<ProfileDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var memberId = currentUserService.UserId ?? throw new UnauthenticatedException();
        var member = await memberRepository.GetByIdAsync(memberId, cancellationToken)
                     ?? throw new MemberNotFoundException(memberId);

        member.Rename(request.Name);

        await memberRepository.UpdateAsync(member, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProfileDto(member.Name, member.Avatar);
    }
}

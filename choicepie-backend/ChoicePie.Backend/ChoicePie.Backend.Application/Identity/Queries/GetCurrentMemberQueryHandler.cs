using ChoicePie.Backend.Application.Identity.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.Queries;

public sealed class GetCurrentMemberQueryHandler(
    IMemberRepository memberRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetCurrentMemberQuery, MemberDto>
{
    public async Task<MemberDto> Handle(GetCurrentMemberQuery request, CancellationToken cancellationToken)
    {
        var memberId = currentUserService.UserId ?? throw new UnauthenticatedException();
        var member = await memberRepository.GetByIdAsync(memberId, cancellationToken)
                     ?? throw new MemberNotFoundException(memberId);

        return MemberDto.FromDomain(member);
    }
}

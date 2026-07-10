using ChoicePie.Backend.Application.Identity.Dtos;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.Queries;

public sealed class GetCurrentMemberQueryHandler(
    IMemberRepository memberRepository,
    IAuthAccountRepository authAccountRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetCurrentMemberQuery, MemberDto>
{
    public async Task<MemberDto> Handle(GetCurrentMemberQuery request, CancellationToken cancellationToken)
    {
        var memberId = currentUserService.UserId ?? throw new UnauthenticatedException();
        var member = await memberRepository.GetByIdAsync(memberId, cancellationToken)
                     ?? throw new MemberNotFoundException(memberId);
        var authAccount = await authAccountRepository.FirstOrDefaultAsync(new AuthAccountByMemberIdSpecification(memberId), cancellationToken)
                          ?? throw new AuthAccountNotFoundException(memberId);

        return MemberDto.FromDomain(member, authAccount);
    }
}

using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Application.Identity.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.Queries;

public sealed class GetCurrentMemberQueryHandler(
    IMemberQueryService memberQueryService,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetCurrentMemberQuery, MemberDto>
{
    public Task<MemberDto> Handle(GetCurrentMemberQuery request, CancellationToken cancellationToken)
    {
        var memberId = currentUserService.UserId ?? throw new UnauthenticatedException();
        return memberQueryService.GetByIdAsync(memberId, cancellationToken);
    }
}

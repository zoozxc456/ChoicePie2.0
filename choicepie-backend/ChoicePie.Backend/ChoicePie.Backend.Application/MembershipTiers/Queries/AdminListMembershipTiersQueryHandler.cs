using ChoicePie.Backend.Application.MembershipTiers.Contracts;
using ChoicePie.Backend.Application.MembershipTiers.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.MembershipTiers.Queries;

public sealed class AdminListMembershipTiersQueryHandler(IMembershipTierQueryService membershipTierQueryService)
    : IRequestHandler<AdminListMembershipTiersQuery, IReadOnlyList<MembershipTierDto>>
{
    public Task<IReadOnlyList<MembershipTierDto>> Handle(AdminListMembershipTiersQuery request, CancellationToken cancellationToken) =>
        membershipTierQueryService.ListAsync(cancellationToken);
}

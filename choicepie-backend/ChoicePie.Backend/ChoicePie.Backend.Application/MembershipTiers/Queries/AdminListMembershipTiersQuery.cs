using ChoicePie.Backend.Application.MembershipTiers.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.MembershipTiers.Queries;

public sealed record AdminListMembershipTiersQuery : IRequest<IReadOnlyList<MembershipTierDto>>;

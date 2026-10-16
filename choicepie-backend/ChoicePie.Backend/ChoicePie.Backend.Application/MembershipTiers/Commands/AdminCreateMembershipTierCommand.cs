using ChoicePie.Backend.Application.MembershipTiers.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.MembershipTiers.Commands;

public sealed record AdminCreateMembershipTierCommand(
    string Name, int DailyGenerationLimit, int DailyTokenBudget) : IRequest<MembershipTierDto>;

using ChoicePie.Backend.Application.MembershipTiers.Commands;

namespace ChoicePie.Backend.WebApi.Requests.AdminMembershipTiers;

public sealed record CreateMembershipTierRequest(string Name, int DailyGenerationLimit, int DailyTokenBudget)
{
    public AdminCreateMembershipTierCommand ToCommand() => new(Name, DailyGenerationLimit, DailyTokenBudget);
}

using ChoicePie.Backend.Application.MembershipTiers.Commands;

namespace ChoicePie.Backend.WebApi.Requests.AdminMembershipTiers;

public sealed record UpdateMembershipTierRequest(string Name, int DailyGenerationLimit, int DailyTokenBudget)
{
    public AdminUpdateMembershipTierCommand ToCommand(Guid tierId) => new(tierId, Name, DailyGenerationLimit, DailyTokenBudget);
}

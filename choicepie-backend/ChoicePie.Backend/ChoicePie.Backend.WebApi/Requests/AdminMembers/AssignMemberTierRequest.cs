using ChoicePie.Backend.Application.AdminMembers.Commands;

namespace ChoicePie.Backend.WebApi.Requests.AdminMembers;

public sealed record AssignMemberTierRequest(Guid TierId)
{
    public AdminAssignMemberTierCommand ToCommand(Guid memberId) => new(memberId, TierId);
}

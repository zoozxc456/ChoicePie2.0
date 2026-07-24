using ChoicePie.Backend.Application.AdminMembers.Commands;

namespace ChoicePie.Backend.WebApi.Requests.AdminMembers;

public sealed record SuspendMemberRequest(string Reason, DateTime? Until)
{
    public AdminSuspendMemberCommand ToCommand(Guid memberId) => new(memberId, Reason, Until);
}

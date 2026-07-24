using MediatR;

namespace ChoicePie.Backend.Application.AdminMembers.Commands;

public sealed record AdminSuspendMemberCommand(Guid MemberId, string Reason, DateTime? Until) : IRequest;

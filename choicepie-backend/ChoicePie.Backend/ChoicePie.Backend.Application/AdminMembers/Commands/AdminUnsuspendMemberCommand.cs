using MediatR;

namespace ChoicePie.Backend.Application.AdminMembers.Commands;

public sealed record AdminUnsuspendMemberCommand(Guid MemberId) : IRequest;

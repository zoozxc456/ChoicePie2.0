using MediatR;

namespace ChoicePie.Backend.Application.AdminMembers.Commands;

public sealed record AdminAssignMemberTierCommand(Guid MemberId, Guid TierId) : IRequest;

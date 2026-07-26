using ChoicePie.Backend.Application.AdminMembers.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.AdminMembers.Queries;

public sealed record AdminGetMemberByIdQuery(Guid Id) : IRequest<AdminMemberDetailDto>;

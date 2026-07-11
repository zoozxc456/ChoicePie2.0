using ChoicePie.Backend.Application.Identity.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.Queries;

public sealed record GetCurrentMemberQuery : IRequest<MemberDto>;

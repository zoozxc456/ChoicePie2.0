using ChoicePie.Backend.Application.AdminMembers.Dtos;
using ChoicePie.Backend.Application.Identity.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.AdminMembers.Queries;

public sealed class AdminGetMemberByIdQueryHandler(IMemberQueryService memberQueryService)
    : IRequestHandler<AdminGetMemberByIdQuery, AdminMemberDetailDto>
{
    public Task<AdminMemberDetailDto> Handle(AdminGetMemberByIdQuery request, CancellationToken cancellationToken) =>
        memberQueryService.AdminGetByIdAsync(request.Id, cancellationToken);
}

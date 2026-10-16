using ChoicePie.Backend.Application.AdminMembers.Dtos;
using ChoicePie.Backend.Application.AiUsage.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.AdminMembers.Queries;

public sealed class AdminGetMemberAiUsageQueryHandler(IAiUsageLogQueryService aiUsageLogQueryService)
    : IRequestHandler<AdminGetMemberAiUsageQuery, AdminMemberAiUsageDto>
{
    private const int RecentLogCount = 20;

    public Task<AdminMemberAiUsageDto> Handle(AdminGetMemberAiUsageQuery request, CancellationToken cancellationToken) =>
        aiUsageLogQueryService.AdminGetByMemberIdAsync(request.MemberId, RecentLogCount, cancellationToken);
}

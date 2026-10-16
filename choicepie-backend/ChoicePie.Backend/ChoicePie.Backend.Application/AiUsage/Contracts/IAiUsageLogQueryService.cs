using ChoicePie.Backend.Application.AdminMembers.Dtos;
using ChoicePie.Backend.Application.AiUsage.Dtos;

namespace ChoicePie.Backend.Application.AiUsage.Contracts;

public interface IAiUsageLogQueryService
{
    Task<AdminMemberAiUsageDto> AdminGetByMemberIdAsync(
        Guid memberId, int recentLogCount, CancellationToken cancellationToken);

    Task<DailyAiUsageDto> GetTodayUsageAsync(Guid memberId, DateTime nowUtc, CancellationToken cancellationToken);
}

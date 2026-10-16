using ChoicePie.Backend.Application.AdminMembers.Dtos;
using ChoicePie.Backend.Application.AiUsage.Contracts;
using ChoicePie.Backend.Application.AiUsage.Dtos;
using ChoicePie.Backend.Domain.Aggregates.AiUsageLog;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;

namespace ChoicePie.Backend.Infrastructure.QueryServices.AiUsage;

public sealed class AiUsageLogQueryService(IReadRepository readRepository, TimeProvider timeProvider)
    : IAiUsageLogQueryService, IScopedDependency
{
    public Task<AdminMemberAiUsageDto> AdminGetByMemberIdAsync(
        Guid memberId, int recentLogCount, CancellationToken cancellationToken)
    {
        var query = readRepository.Query<AiUsageLog>().Where(l => l.MemberId == memberId);

        var totalTokensUsed = query.Sum(l => (int?)l.TokensUsed) ?? 0;
        var totalGenerationCount = query.Count();

        var today = timeProvider.GetUtcNow().UtcDateTime.Date;
        var tomorrow = today.AddDays(1);
        var todayQuery = query.Where(l => l.CreatedAt >= today && l.CreatedAt < tomorrow);
        var todayGenerationCount = todayQuery.Count();
        var todayTokensUsed = todayQuery.Sum(l => (int?)l.TokensUsed) ?? 0;

        var recentLogs = query
            .OrderByDescending(l => l.CreatedAt)
            .Take(recentLogCount)
            .Select(l => new AdminAiUsageLogEntryDto(l.Id, l.Provider, l.Model, l.TokensUsed, l.CreatedAt))
            .ToList();

        return Task.FromResult(new AdminMemberAiUsageDto(
            memberId, totalTokensUsed, totalGenerationCount, todayGenerationCount, todayTokensUsed, recentLogs));
    }

    public Task<DailyAiUsageDto> GetTodayUsageAsync(Guid memberId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var today = nowUtc.Date;
        var tomorrow = today.AddDays(1);

        var query = readRepository.Query<AiUsageLog>()
            .Where(l => l.MemberId == memberId && l.CreatedAt >= today && l.CreatedAt < tomorrow);

        var generationCount = query.Count();
        var tokensUsed = query.Sum(l => (int?)l.TokensUsed) ?? 0;

        return Task.FromResult(new DailyAiUsageDto(generationCount, tokensUsed));
    }
}

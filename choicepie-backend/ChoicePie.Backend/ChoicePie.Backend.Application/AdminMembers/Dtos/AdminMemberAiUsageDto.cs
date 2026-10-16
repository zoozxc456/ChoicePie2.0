namespace ChoicePie.Backend.Application.AdminMembers.Dtos;

public sealed record AdminMemberAiUsageDto(
    Guid MemberId,
    int TotalTokensUsed,
    int TotalGenerationCount,
    int TodayGenerationCount,
    int TodayTokensUsed,
    IReadOnlyList<AdminAiUsageLogEntryDto> RecentLogs);

public sealed record AdminAiUsageLogEntryDto(
    Guid Id,
    string Provider,
    string Model,
    int TokensUsed,
    DateTime CreatedAt);

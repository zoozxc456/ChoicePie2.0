namespace ChoicePie.Backend.Application.Identity.Dtos;

public sealed record AdminMemberDashboardStatsDto(
    int TotalCount,
    int SuspendedCount,
    int NewCountLast7Days);

namespace ChoicePie.Backend.Application.AdminDashboard.Dtos;

public sealed record AdminDashboardSummaryDto(
    int PendingQuizReportCount,
    int TotalMemberCount,
    int SuspendedMemberCount,
    int NewMemberCountLast7Days,
    int TotalQuizCount,
    int TakenDownQuizCount,
    int NewQuizCountLast7Days,
    int TakenDownQuizCountLast7Days,
    DateTime GeneratedAt);

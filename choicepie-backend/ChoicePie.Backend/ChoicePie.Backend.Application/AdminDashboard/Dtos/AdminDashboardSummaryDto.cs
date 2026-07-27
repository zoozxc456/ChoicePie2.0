using ChoicePie.Backend.Application.Quizzes.Dtos;

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
    IReadOnlyList<DailyCountDto> NewMembersByDay,
    IReadOnlyList<DailyCountDto> NewQuizzesByDay,
    IReadOnlyList<DailyCountDto> TakenDownQuizzesByDay,
    IReadOnlyList<QuizSummaryDto> TopQuizzes,
    DateTime GeneratedAt);

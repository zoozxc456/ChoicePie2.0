using ChoicePie.Backend.Application.AdminDashboard.Dtos;

namespace ChoicePie.Backend.Application.Quizzes.Dtos;

public sealed record AdminQuizDashboardStatsDto(
    int TotalCount,
    int TakenDownCount,
    int NewCountLast7Days,
    int TakenDownCountLast7Days,
    IReadOnlyList<DailyCountDto> NewQuizzesByDay,
    IReadOnlyList<DailyCountDto> TakenDownQuizzesByDay);

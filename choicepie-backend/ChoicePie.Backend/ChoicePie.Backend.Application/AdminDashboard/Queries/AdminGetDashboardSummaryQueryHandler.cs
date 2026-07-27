using ChoicePie.Backend.Application.AdminDashboard.Dtos;
using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Application.QuizReports.Contracts;
using ChoicePie.Backend.Application.Quizzes.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.AdminDashboard.Queries;

public sealed class AdminGetDashboardSummaryQueryHandler(
    IMemberQueryService memberQueryService,
    IQuizQueryService quizQueryService,
    IQuizReportQueryService quizReportQueryService,
    TimeProvider timeProvider)
    : IRequestHandler<AdminGetDashboardSummaryQuery, AdminDashboardSummaryDto>
{
    public async Task<AdminDashboardSummaryDto> Handle(AdminGetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var memberStatsTask = memberQueryService.AdminGetDashboardStatsAsync(cancellationToken);
        var quizStatsTask = quizQueryService.AdminGetDashboardStatsAsync(cancellationToken);
        var pendingReportCountTask = quizReportQueryService.AdminGetPendingCountAsync(cancellationToken);
        var topQuizzesTask = quizQueryService.AdminGetTopQuizzesAsync(10, cancellationToken);

        await Task.WhenAll(memberStatsTask, quizStatsTask, pendingReportCountTask, topQuizzesTask);

        var memberStats = memberStatsTask.Result;
        var quizStats = quizStatsTask.Result;

        return new AdminDashboardSummaryDto(
            pendingReportCountTask.Result,
            memberStats.TotalCount,
            memberStats.SuspendedCount,
            memberStats.NewCountLast7Days,
            quizStats.TotalCount,
            quizStats.TakenDownCount,
            quizStats.NewCountLast7Days,
            quizStats.TakenDownCountLast7Days,
            memberStats.NewMembersByDay,
            quizStats.NewQuizzesByDay,
            quizStats.TakenDownQuizzesByDay,
            topQuizzesTask.Result,
            timeProvider.GetUtcNow().UtcDateTime);
    }
}

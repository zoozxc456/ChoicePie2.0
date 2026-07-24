using ChoicePie.Backend.Application.QuizReports.Contracts;
using ChoicePie.Backend.Application.QuizReports.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.QuizReport;
using ChoicePie.Backend.Domain.Aggregates.QuizReport.Enums;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;

namespace ChoicePie.Backend.Infrastructure.QueryServices.QuizReports;

public sealed class QuizReportQueryService(IReadRepository readRepository) : IQuizReportQueryService, IScopedDependency
{
    public Task<PagedResult<QuizReportDto>> AdminListAsync(
        string? status, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = readRepository.Query<QuizReport>();

        if (!string.IsNullOrWhiteSpace(status))
        {
            var reportStatus = ReportStatus.FromName(status);
            if (reportStatus is not null)
            {
                query = query.Where(r => r.Status == reportStatus);
            }
        }

        var totalCount = query.Count();

        var joined =
            from r in query
            join q in readRepository.Query<Quiz>() on r.QuizId equals q.Id into quizGroup
            from quiz in quizGroup.DefaultIfEmpty()
            join m in readRepository.Query<Member>() on r.CreatorId!.Value equals m.Id into reporterGroup
            from reporter in reporterGroup.DefaultIfEmpty()
            orderby r.CreatedAt descending
            select new QuizReportDto(
                r.Id,
                r.QuizId,
                quiz != null ? quiz.Title : "已刪除的題庫",
                r.ReporterId,
                reporter != null ? reporter.Name : "Unknown",
                r.Reason.Name,
                r.Description,
                r.Status.Name,
                r.ResolvedBy,
                r.ResolvedAt,
                r.ResolutionNote,
                r.CreatedAt);

        var items = joined
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PagedResult<QuizReportDto>(items, pageNumber, pageSize, totalCount));
    }
}

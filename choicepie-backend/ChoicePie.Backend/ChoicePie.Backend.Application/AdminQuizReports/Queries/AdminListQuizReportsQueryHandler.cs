using ChoicePie.Backend.Application.QuizReports.Contracts;
using ChoicePie.Backend.Application.QuizReports.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.AdminQuizReports.Queries;

public sealed class AdminListQuizReportsQueryHandler(IQuizReportQueryService quizReportQueryService)
    : IRequestHandler<AdminListQuizReportsQuery, PagedResult<QuizReportDto>>
{
    public Task<PagedResult<QuizReportDto>> Handle(AdminListQuizReportsQuery request, CancellationToken cancellationToken) =>
        quizReportQueryService.AdminListAsync(request.Status, request.PageNumber, request.PageSize, cancellationToken);
}

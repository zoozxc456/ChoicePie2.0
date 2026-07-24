using ChoicePie.Backend.Application.QuizReports.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.AdminQuizReports.Queries;

public sealed class AdminListQuizReportsQuery : PaginationParameters, IRequest<PagedResult<QuizReportDto>>
{
    public string? Status { get; set; }
}

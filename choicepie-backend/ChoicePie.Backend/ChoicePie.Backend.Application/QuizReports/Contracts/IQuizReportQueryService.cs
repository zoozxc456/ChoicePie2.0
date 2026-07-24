using ChoicePie.Backend.Application.QuizReports.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;

namespace ChoicePie.Backend.Application.QuizReports.Contracts;

public interface IQuizReportQueryService
{
    Task<PagedResult<QuizReportDto>> AdminListAsync(
        string? status, int pageNumber, int pageSize, CancellationToken cancellationToken);
}

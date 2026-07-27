using ChoicePie.Backend.Application.AdminQuizReports.Commands;

namespace ChoicePie.Backend.WebApi.Requests.AdminQuizReports;

public sealed record DismissQuizReportRequest(string? Note)
{
    public AdminDismissQuizReportCommand ToCommand(Guid reportId) => new(reportId, Note);
}

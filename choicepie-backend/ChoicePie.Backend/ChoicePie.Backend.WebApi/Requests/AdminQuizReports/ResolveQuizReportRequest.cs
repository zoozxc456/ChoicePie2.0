using ChoicePie.Backend.Application.AdminQuizReports.Commands;

namespace ChoicePie.Backend.WebApi.Requests.AdminQuizReports;

public sealed record ResolveQuizReportRequest(string? Note)
{
    public AdminResolveQuizReportCommand ToCommand(Guid reportId) => new(reportId, Note);
}

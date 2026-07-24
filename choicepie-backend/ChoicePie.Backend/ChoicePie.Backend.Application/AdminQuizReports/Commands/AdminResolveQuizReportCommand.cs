using MediatR;

namespace ChoicePie.Backend.Application.AdminQuizReports.Commands;

public sealed record AdminResolveQuizReportCommand(Guid ReportId, string? Note) : IRequest;

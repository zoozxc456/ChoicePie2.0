using MediatR;

namespace ChoicePie.Backend.Application.AdminQuizReports.Commands;

public sealed record AdminDismissQuizReportCommand(Guid ReportId, string? Note) : IRequest;

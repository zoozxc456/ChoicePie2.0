namespace ChoicePie.Backend.Application.QuizReports.Dtos;

public sealed record QuizReportDto(
    Guid Id,
    Guid QuizId,
    string QuizTitle,
    Guid ReporterId,
    string ReporterName,
    string Reason,
    string? Description,
    string Status,
    Guid? ResolvedBy,
    DateTime? ResolvedAt,
    string? ResolutionNote,
    DateTime CreatedAt);

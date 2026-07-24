using MediatR;

namespace ChoicePie.Backend.Application.QuizReports.Commands;

public sealed record CreateQuizReportCommand(Guid QuizId, string Reason, string? Description) : IRequest;

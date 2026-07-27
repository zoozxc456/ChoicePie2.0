using ChoicePie.Backend.Application.QuizReports.Commands;

namespace ChoicePie.Backend.WebApi.Requests.Quizzes;

public sealed record CreateQuizReportRequest(string Reason, string? Description)
{
    public CreateQuizReportCommand ToCommand(Guid quizId) => new(quizId, Reason, Description);
}

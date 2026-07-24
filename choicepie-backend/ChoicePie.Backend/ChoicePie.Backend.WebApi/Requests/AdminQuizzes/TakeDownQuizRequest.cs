using ChoicePie.Backend.Application.AdminQuizzes.Commands;

namespace ChoicePie.Backend.WebApi.Requests.AdminQuizzes;

public sealed record TakeDownQuizRequest(string Reason)
{
    public AdminTakeDownQuizCommand ToCommand(Guid quizId) => new(quizId, Reason);
}

using ChoicePie.Backend.Application.QuizAttempts.Commands;

namespace ChoicePie.Backend.WebApi.Requests.QuizAttempts;

public sealed record StartQuizAttemptRequest(Guid QuizId)
{
    public StartQuizAttemptCommand ToCommand() => new(QuizId);
}

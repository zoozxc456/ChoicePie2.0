using ChoicePie.Backend.Application.QuizAttempts.Commands;

namespace ChoicePie.Backend.WebApi.Requests.QuizAttempts;

public sealed record SubmitQuizAttemptAnswerRequest(Guid QuestionId, int SelectedOptionIndex)
{
    public SubmitQuizAttemptAnswerCommand ToCommand(Guid attemptId) => new(attemptId, QuestionId, SelectedOptionIndex);
}

using ChoicePie.Backend.Application.Quizzes.Commands;

namespace ChoicePie.Backend.WebApi.Requests.Quizzes;

public sealed record UpdateQuestionRequest(string Text, IReadOnlyList<string> Options, int AnswerIndex, string Explanation)
{
    public UpdateQuestionCommand ToCommand(Guid quizId, Guid questionId) => new()
    {
        QuizId = quizId,
        QuestionId = questionId,
        Text = Text,
        Options = Options,
        AnswerIndex = AnswerIndex,
        Explanation = Explanation
    };
}

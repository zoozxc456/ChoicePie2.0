using ChoicePie.Backend.Application.Quizzes.Commands;

namespace ChoicePie.Backend.WebApi.Requests.Quizzes;

public sealed record AddQuestionRequest(string Text, IReadOnlyList<string> Options, int AnswerIndex, string Explanation)
{
    public AddQuestionCommand ToCommand(Guid quizId) => new()
    {
        QuizId = quizId,
        Text = Text,
        Options = Options,
        AnswerIndex = AnswerIndex,
        Explanation = Explanation
    };
}

using ChoicePie.Backend.Application.Quizzes.Commands;

namespace ChoicePie.Backend.WebApi.Requests.Quizzes;

public sealed record GenerateQuizQuestionsRequest(string Content, int QuestionCount, string Difficulty)
{
    public GenerateQuizQuestionsCommand ToCommand() => new()
    {
        Content = Content,
        QuestionCount = QuestionCount,
        Difficulty = Difficulty
    };
}

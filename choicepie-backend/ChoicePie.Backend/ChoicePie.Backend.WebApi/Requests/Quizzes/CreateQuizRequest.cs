using ChoicePie.Backend.Application.Quizzes.Commands;

namespace ChoicePie.Backend.WebApi.Requests.Quizzes;

public sealed record CreateQuestionRequestItem(string Text, IReadOnlyList<string> Options, int AnswerIndex, string Explanation)
{
    public CreateQuestionDto ToDto() => new(Text, Options, AnswerIndex, Explanation);
}

public sealed record CreateQuizRequest(
    string Title,
    string? Description,
    string CoverEmoji,
    string CoverGradient,
    string Difficulty,
    IReadOnlyList<string> Tags,
    IReadOnlyList<CreateQuestionRequestItem> Questions)
{
    public CreateQuizCommand ToCommand() => new()
    {
        Title = Title,
        Description = Description,
        CoverEmoji = CoverEmoji,
        CoverGradient = CoverGradient,
        Difficulty = Difficulty,
        Tags = Tags,
        Questions = Questions.Select(q => q.ToDto()).ToList()
    };
}

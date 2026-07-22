using ChoicePie.Backend.Application.Quizzes.Commands;

namespace ChoicePie.Backend.WebApi.Requests.Quizzes;

public sealed record UpdateQuizRequest(string Title, string? Description, IReadOnlyList<string> Tags)
{
    public UpdateQuizCommand ToCommand(Guid id) => new()
    {
        Id = id,
        Title = Title,
        Description = Description,
        Tags = Tags
    };
}

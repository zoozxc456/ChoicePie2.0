using ChoicePie.Backend.Application.Comments.Commands;

namespace ChoicePie.Backend.WebApi.Requests.Quizzes;

public sealed record AddCommentRequest(string Text)
{
    public AddCommentCommand ToCommand(Guid quizId) => new(quizId, Text);
}

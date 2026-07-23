using ChoicePie.Backend.Application.Comments.Commands;

namespace ChoicePie.Backend.WebApi.Requests.Quizzes;

public sealed record UpdateCommentRequest(string Text)
{
    public UpdateCommentCommand ToCommand(Guid commentId) => new(commentId, Text);
}

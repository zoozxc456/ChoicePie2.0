using ChoicePie.Backend.Domain.Aggregates.Comment.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.Comment;

public sealed class Comment : AggregateRoot<Guid>
{
    private const int MaxTextLength = 1000;

    public Guid QuizId { get; private set; }
    public string Text { get; private set; } = null!;
    public Guid UserId => CreatorId!.Value;

    private Comment()
    {
    }

    public static Comment Create(Guid quizId, Guid userId, string text)
    {
        var trimmed = text?.Trim() ?? string.Empty;

        if (trimmed.Length == 0 || trimmed.Length > MaxTextLength)
        {
            throw new InvalidCommentTextException(trimmed.Length, MaxTextLength);
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            QuizId = quizId,
            Text = trimmed
        };

        comment.SetCreated(userId);

        return comment;
    }

    public void EnsureModifiableBy(Guid userId)
    {
        if (UserId != userId)
        {
            throw new CommentForbiddenException(Id, userId);
        }
    }

    public void UpdateText(string text)
    {
        var trimmed = text?.Trim() ?? string.Empty;

        if (trimmed.Length == 0 || trimmed.Length > MaxTextLength)
        {
            throw new InvalidCommentTextException(trimmed.Length, MaxTextLength);
        }

        Text = trimmed;
        Touch();
    }
}

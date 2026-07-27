using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Comment.Exceptions;

public sealed class CommentNotFoundException(Guid commentId)
    : DomainException(
        internalLogMessage: $"Comment {commentId} not found.",
        presentationMessage: "找不到指定的留言。",
        errorCode: "COMMENT_NOT_FOUND",
        statusCode: HttpStatusCode.NotFound);

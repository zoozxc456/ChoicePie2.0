using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Comment.Exceptions;

public sealed class CommentForbiddenException(Guid commentId, Guid userId)
    : DomainException(
        internalLogMessage: $"User {userId} is not allowed to modify comment {commentId}.",
        presentationMessage: "您沒有權限修改此留言。",
        errorCode: "COMMENT_FORBIDDEN",
        statusCode: HttpStatusCode.Forbidden);

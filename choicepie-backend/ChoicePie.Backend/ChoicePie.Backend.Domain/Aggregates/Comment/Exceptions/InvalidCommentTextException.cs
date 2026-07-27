using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Comment.Exceptions;

public sealed class InvalidCommentTextException(int length, int maxLength)
    : DomainException(
        internalLogMessage: $"Comment text length {length} is invalid (max {maxLength}).",
        presentationMessage: "留言內容不可為空，且長度不可超過限制。",
        errorCode: "COMMENT_INVALID_TEXT",
        statusCode: HttpStatusCode.BadRequest);

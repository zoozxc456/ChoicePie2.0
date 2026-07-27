using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.CreatorFollow.Exceptions;

public sealed class CannotFollowSelfException(Guid userId)
    : DomainException(
        internalLogMessage: $"User {userId} attempted to follow themselves.",
        presentationMessage: "無法追蹤自己。",
        errorCode: "CREATOR_FOLLOW_SELF",
        statusCode: HttpStatusCode.BadRequest);

using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;

public sealed class UnauthenticatedException()
    : DomainException(
        internalLogMessage: "Current user is not authenticated.",
        presentationMessage: "請先登入。",
        errorCode: "UNAUTHENTICATED",
        statusCode: HttpStatusCode.Unauthorized);

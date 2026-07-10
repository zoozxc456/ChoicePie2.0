using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;

public sealed class UnauthenticatedException()
    : DomainException(
        internalLogMessage: "Current admin user is not authenticated.",
        presentationMessage: "請先登入後台。",
        errorCode: "ADMIN_UNAUTHENTICATED",
        statusCode: HttpStatusCode.Unauthorized);

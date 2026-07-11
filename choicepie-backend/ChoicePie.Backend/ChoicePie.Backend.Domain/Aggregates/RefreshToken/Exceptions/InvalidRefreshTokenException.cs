using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.RefreshToken.Exceptions;

public sealed class InvalidRefreshTokenException()
    : DomainException(
        internalLogMessage: "Refresh token is missing, expired, or has already been revoked.",
        presentationMessage: "登入已過期，請重新登入。",
        errorCode: "INVALID_REFRESH_TOKEN",
        statusCode: HttpStatusCode.Unauthorized);

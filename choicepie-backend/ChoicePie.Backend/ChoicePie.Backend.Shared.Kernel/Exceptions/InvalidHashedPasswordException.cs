using System.Net;

namespace ChoicePie.Backend.Shared.Kernel.Exceptions;

public sealed class InvalidHashedPasswordException(string reason)
    : DomainException(
        internalLogMessage: $"Invalid hashed password: {reason}",
        presentationMessage: reason,
        errorCode: "INVALID_HASHED_PASSWORD",
        statusCode: HttpStatusCode.BadRequest);

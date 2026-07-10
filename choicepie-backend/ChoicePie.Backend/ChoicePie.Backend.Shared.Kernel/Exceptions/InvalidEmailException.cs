using System.Net;

namespace ChoicePie.Backend.Shared.Kernel.Exceptions;

public sealed class InvalidEmailException(string value)
    : DomainException(
        internalLogMessage: $"Invalid email format: {value}",
        presentationMessage: "Email 格式不正確。",
        errorCode: "INVALID_EMAIL",
        statusCode: HttpStatusCode.BadRequest);

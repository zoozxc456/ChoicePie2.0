using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;

public sealed class InvalidCredentialsException()
    : DomainException(
        internalLogMessage: "Invalid email or password.",
        presentationMessage: "帳號或密碼錯誤。",
        errorCode: "INVALID_CREDENTIALS",
        statusCode: HttpStatusCode.Unauthorized);

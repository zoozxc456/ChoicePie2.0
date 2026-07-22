using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Exceptions;

public sealed class InvalidCredentialsException()
    : DomainException(
        internalLogMessage: "Invalid admin email or password.",
        presentationMessage: "帳號或密碼錯誤。",
        errorCode: "ADMIN_INVALID_CREDENTIALS",
        statusCode: HttpStatusCode.Unauthorized);

using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;

public sealed class InvalidGoogleTokenException()
    : DomainException(
        internalLogMessage: "Invalid or unverifiable Google ID token.",
        presentationMessage: "Google 登入驗證失敗，請重新嘗試。",
        errorCode: "INVALID_GOOGLE_TOKEN",
        statusCode: HttpStatusCode.Unauthorized);

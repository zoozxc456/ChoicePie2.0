using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;

public sealed class NoOriginalLoginMethodException(Guid authAccountId)
    : DomainException(
        internalLogMessage: $"AuthAccount {authAccountId} has no original (email/password) login method.",
        presentationMessage: "此帳號未使用密碼登入方式，無法重設密碼。",
        errorCode: "NO_ORIGINAL_LOGIN_METHOD",
        statusCode: HttpStatusCode.BadRequest);

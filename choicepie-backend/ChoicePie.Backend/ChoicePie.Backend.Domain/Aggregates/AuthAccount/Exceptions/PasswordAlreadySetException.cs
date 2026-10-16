using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;

public sealed class PasswordAlreadySetException(Guid authAccountId)
    : DomainException(
        internalLogMessage: $"AuthAccount {authAccountId} already has an original (email/password) login method.",
        presentationMessage: "此帳號已設定密碼。",
        errorCode: "PASSWORD_ALREADY_SET",
        statusCode: HttpStatusCode.Conflict);

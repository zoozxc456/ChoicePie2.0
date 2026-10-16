using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;

public sealed class CannotRemoveLastLoginMethodException(Guid authAccountId)
    : DomainException(
        internalLogMessage: $"AuthAccount {authAccountId} cannot remove its last remaining login method.",
        presentationMessage: "這是您唯一的登入方式，無法解除連結。請先設定密碼或連結其他登入方式。",
        errorCode: "CANNOT_REMOVE_LAST_LOGIN_METHOD",
        statusCode: HttpStatusCode.Conflict);

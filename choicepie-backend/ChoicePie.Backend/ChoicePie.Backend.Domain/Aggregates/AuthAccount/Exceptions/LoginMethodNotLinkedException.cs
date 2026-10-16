using System.Net;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;

public sealed class LoginMethodNotLinkedException(LoginProvider provider)
    : DomainException(
        internalLogMessage: $"Login provider not linked (or not removable): {provider.Name}",
        presentationMessage: "尚未連結此登入方式。",
        errorCode: "LOGIN_METHOD_NOT_LINKED",
        statusCode: HttpStatusCode.BadRequest);

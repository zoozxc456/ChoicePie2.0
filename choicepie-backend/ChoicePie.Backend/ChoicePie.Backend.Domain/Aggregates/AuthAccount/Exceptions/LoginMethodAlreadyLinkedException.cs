using System.Net;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;

public sealed class LoginMethodAlreadyLinkedException(LoginProvider provider)
    : DomainException(
        internalLogMessage: $"Login provider already linked: {provider.Name}",
        presentationMessage: "此登入方式已連結。",
        errorCode: "LOGIN_METHOD_ALREADY_LINKED",
        statusCode: HttpStatusCode.Conflict);

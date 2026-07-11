using System.Net;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Enums;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Exceptions;

public sealed class AdminLoginMethodAlreadyLinkedException(AdminLoginProvider provider)
    : DomainException(
        internalLogMessage: $"Admin login provider already linked: {provider.Name}",
        presentationMessage: "此登入方式已連結。",
        errorCode: "ADMIN_LOGIN_METHOD_ALREADY_LINKED",
        statusCode: HttpStatusCode.Conflict);

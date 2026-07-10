using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Exceptions;

public sealed class EmailAlreadyRegisteredException(string email)
    : DomainException(
        internalLogMessage: $"Admin email already registered: {email}",
        presentationMessage: "此 Email 已被註冊。",
        errorCode: "ADMIN_EMAIL_ALREADY_REGISTERED",
        statusCode: HttpStatusCode.Conflict);

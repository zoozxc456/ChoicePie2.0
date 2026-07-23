using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Exceptions;

public sealed class AdminAuthAccountNotFoundException(Guid adminUserId)
    : DomainException(
        internalLogMessage: $"AdminAuthAccount for admin user {adminUserId} not found.",
        presentationMessage: "找不到後台帳號。",
        errorCode: "ADMIN_AUTH_ACCOUNT_NOT_FOUND",
        statusCode: HttpStatusCode.NotFound);

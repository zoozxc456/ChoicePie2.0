using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;

public sealed class AdminUserNotFoundException(Guid adminUserId)
    : DomainException(
        internalLogMessage: $"AdminUser {adminUserId} not found.",
        presentationMessage: "找不到後台使用者。",
        errorCode: "ADMIN_USER_NOT_FOUND",
        statusCode: HttpStatusCode.NotFound);

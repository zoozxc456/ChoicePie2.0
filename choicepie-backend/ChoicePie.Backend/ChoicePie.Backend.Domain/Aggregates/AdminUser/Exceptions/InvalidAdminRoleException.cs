using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;

public sealed class InvalidAdminRoleException(string role)
    : DomainException(
        internalLogMessage: $"Invalid admin role: {role}",
        presentationMessage: "無效的角色。",
        errorCode: "INVALID_ADMIN_ROLE",
        statusCode: HttpStatusCode.BadRequest);

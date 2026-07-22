using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;

public sealed class InvalidAdminUserNameException(string name)
    : DomainException(
        internalLogMessage: $"Invalid admin user name: {name}",
        presentationMessage: "姓名長度不符規定。",
        errorCode: "INVALID_ADMIN_USER_NAME",
        statusCode: HttpStatusCode.BadRequest);

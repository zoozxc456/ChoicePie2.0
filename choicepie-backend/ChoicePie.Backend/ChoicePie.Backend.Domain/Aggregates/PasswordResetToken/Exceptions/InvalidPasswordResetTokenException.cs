using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.PasswordResetToken.Exceptions;

public sealed class InvalidPasswordResetTokenException()
    : DomainException(
        internalLogMessage: "Password reset token is missing, expired, or has already been used.",
        presentationMessage: "重設密碼連結已失效，請重新申請。",
        errorCode: "INVALID_PASSWORD_RESET_TOKEN",
        statusCode: HttpStatusCode.BadRequest);

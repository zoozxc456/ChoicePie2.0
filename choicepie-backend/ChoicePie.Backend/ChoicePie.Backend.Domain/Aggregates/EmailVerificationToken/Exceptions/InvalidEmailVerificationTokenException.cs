using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken.Exceptions;

public sealed class InvalidEmailVerificationTokenException()
    : DomainException(
        internalLogMessage: "Email verification token is missing, expired, or has already been used.",
        presentationMessage: "驗證連結已失效，請重新發送驗證信。",
        errorCode: "INVALID_EMAIL_VERIFICATION_TOKEN",
        statusCode: HttpStatusCode.BadRequest);

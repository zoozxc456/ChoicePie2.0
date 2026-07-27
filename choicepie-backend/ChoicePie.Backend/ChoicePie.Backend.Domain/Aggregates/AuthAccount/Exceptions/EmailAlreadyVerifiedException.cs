using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;

public sealed class EmailAlreadyVerifiedException()
    : DomainException(
        internalLogMessage: "AuthAccount email is already verified.",
        presentationMessage: "此帳號的 Email 已經完成驗證。",
        errorCode: "EMAIL_ALREADY_VERIFIED",
        statusCode: HttpStatusCode.BadRequest);

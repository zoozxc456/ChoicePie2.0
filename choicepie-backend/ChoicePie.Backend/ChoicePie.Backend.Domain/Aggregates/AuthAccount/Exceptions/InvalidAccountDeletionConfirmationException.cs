using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;

public sealed class InvalidAccountDeletionConfirmationException()
    : DomainException(
        internalLogMessage: "Account deletion confirmation (email) did not match.",
        presentationMessage: "確認用的電子郵件與帳號不符。",
        errorCode: "INVALID_ACCOUNT_DELETION_CONFIRMATION",
        statusCode: HttpStatusCode.BadRequest);

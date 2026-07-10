using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;

public sealed class AuthAccountNotFoundException(Guid memberId)
    : DomainException(
        internalLogMessage: $"AuthAccount for member {memberId} not found.",
        presentationMessage: "找不到帳號。",
        errorCode: "AUTH_ACCOUNT_NOT_FOUND",
        statusCode: HttpStatusCode.NotFound);

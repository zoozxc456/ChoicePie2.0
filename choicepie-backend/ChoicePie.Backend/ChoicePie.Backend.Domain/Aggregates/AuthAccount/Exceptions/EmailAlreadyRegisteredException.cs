using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;

public sealed class EmailAlreadyRegisteredException(string email)
    : DomainException(
        internalLogMessage: $"Email already registered: {email}",
        presentationMessage: "此 Email 已被註冊。",
        errorCode: "EMAIL_ALREADY_REGISTERED",
        statusCode: HttpStatusCode.Conflict);

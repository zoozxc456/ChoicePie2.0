using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;

public sealed class InvalidMemberSuspensionException(string message)
    : DomainException(
        internalLogMessage: message,
        presentationMessage: message,
        errorCode: "INVALID_MEMBER_SUSPENSION",
        statusCode: HttpStatusCode.BadRequest);

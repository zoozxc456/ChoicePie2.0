using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;

public sealed class MemberNotFoundException(Guid memberId)
    : DomainException(
        internalLogMessage: $"Member {memberId} not found.",
        presentationMessage: "找不到會員。",
        errorCode: "MEMBER_NOT_FOUND",
        statusCode: HttpStatusCode.NotFound);

using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.MembershipTier.Exceptions;

public sealed class InvalidMembershipTierException(string message)
    : DomainException(
        internalLogMessage: message,
        presentationMessage: message,
        errorCode: "INVALID_MEMBERSHIP_TIER",
        statusCode: HttpStatusCode.BadRequest);

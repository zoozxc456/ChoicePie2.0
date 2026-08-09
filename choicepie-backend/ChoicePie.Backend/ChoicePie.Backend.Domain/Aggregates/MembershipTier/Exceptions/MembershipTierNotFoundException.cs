using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.MembershipTier.Exceptions;

public sealed class MembershipTierNotFoundException(Guid tierId)
    : DomainException(
        internalLogMessage: $"MembershipTier {tierId} not found.",
        presentationMessage: "找不到會員等級。",
        errorCode: "MEMBERSHIP_TIER_NOT_FOUND",
        statusCode: HttpStatusCode.NotFound);

using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken.Events;

public sealed record EmailVerificationRequestedDomainEvent(Guid AuthAccountId, string Email, string RawToken) : BaseDomainEvent;

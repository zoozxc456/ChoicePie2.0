using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.PasswordResetToken.Events;

public sealed record PasswordResetRequestedDomainEvent(Guid AuthAccountId, string Email, string RawToken) : BaseDomainEvent;

using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Events;

public sealed record AdminAuthAccountCreatedDomainEvent(Guid AdminAuthAccountId, Guid AdminUserId, string Email) : BaseDomainEvent;

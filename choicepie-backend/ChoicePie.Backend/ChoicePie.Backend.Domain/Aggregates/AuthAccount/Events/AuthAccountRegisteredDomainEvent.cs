using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.AuthAccount.Events;

public sealed record AuthAccountRegisteredDomainEvent(Guid AuthAccountId, Guid MemberId, string Email) : BaseDomainEvent;

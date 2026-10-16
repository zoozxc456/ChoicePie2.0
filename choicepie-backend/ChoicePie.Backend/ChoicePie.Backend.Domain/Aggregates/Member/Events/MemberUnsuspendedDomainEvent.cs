using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.Member.Events;

public sealed record MemberUnsuspendedDomainEvent(Guid MemberId) : BaseDomainEvent;

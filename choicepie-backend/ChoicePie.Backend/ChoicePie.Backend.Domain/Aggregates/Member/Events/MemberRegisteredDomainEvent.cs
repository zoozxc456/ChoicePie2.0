using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.Member.Events;

public sealed record MemberRegisteredDomainEvent(Guid MemberId, string Email, string Name) : BaseDomainEvent;

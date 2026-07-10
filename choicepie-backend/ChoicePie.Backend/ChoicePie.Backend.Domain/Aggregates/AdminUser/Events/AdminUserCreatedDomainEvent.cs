using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.AdminUser.Events;

public sealed record AdminUserCreatedDomainEvent(Guid AdminUserId, string Name, string Role) : BaseDomainEvent;

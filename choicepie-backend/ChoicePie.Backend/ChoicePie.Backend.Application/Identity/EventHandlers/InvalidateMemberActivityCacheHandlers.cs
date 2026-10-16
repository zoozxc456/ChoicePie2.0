using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Domain.Aggregates.Member.Events;
using ChoicePie.Backend.Shared.Application.Events;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.EventHandlers;

// 會員狀態（刪除／停權／解除停權）改變後立刻清掉活躍狀態快取，讓既有 access token 馬上被拒絕或恢復。
public sealed class InvalidateMemberActivityCacheOnAccountClosedHandler(IMemberActivityCache cache)
    : INotificationHandler<DomainEventNotification<MemberAccountClosedDomainEvent>>
{
    public Task Handle(DomainEventNotification<MemberAccountClosedDomainEvent> notification,
        CancellationToken cancellationToken) =>
        cache.InvalidateAsync(notification.DomainEvent.MemberId, cancellationToken);
}

public sealed class InvalidateMemberActivityCacheOnSuspendedHandler(IMemberActivityCache cache)
    : INotificationHandler<DomainEventNotification<MemberSuspendedDomainEvent>>
{
    public Task Handle(DomainEventNotification<MemberSuspendedDomainEvent> notification,
        CancellationToken cancellationToken) =>
        cache.InvalidateAsync(notification.DomainEvent.MemberId, cancellationToken);
}

public sealed class InvalidateMemberActivityCacheOnUnsuspendedHandler(IMemberActivityCache cache)
    : INotificationHandler<DomainEventNotification<MemberUnsuspendedDomainEvent>>
{
    public Task Handle(DomainEventNotification<MemberUnsuspendedDomainEvent> notification,
        CancellationToken cancellationToken) =>
        cache.InvalidateAsync(notification.DomainEvent.MemberId, cancellationToken);
}

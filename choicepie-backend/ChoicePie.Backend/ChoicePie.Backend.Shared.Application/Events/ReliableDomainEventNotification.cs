using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;
using MediatR;

namespace ChoicePie.Backend.Shared.Application.Events;

/// <summary>
/// 由 Outbox 派送的領域事件：事件跟業務資料在同一個交易寫入 outbox，commit 後由背景 processor
/// 送給 handler，失敗會退避重試，所以 process 掛掉也不會遺失。代價是延遲數秒、at-least-once ——
/// handler 必須能承受同一事件被送不只一次（可用 <see cref="BaseDomainEvent.EventId"/> 去重）。
///
/// 需要「commit 後立刻生效」的 handler（例如清快取）請改訂 <see cref="DomainEventNotification{TDomainEvent}"/>。
/// </summary>
public sealed class ReliableDomainEventNotification<TDomainEvent>(TDomainEvent domainEvent)
    : INotification where TDomainEvent : BaseDomainEvent
{
    public TDomainEvent DomainEvent { get; } = domainEvent;
}

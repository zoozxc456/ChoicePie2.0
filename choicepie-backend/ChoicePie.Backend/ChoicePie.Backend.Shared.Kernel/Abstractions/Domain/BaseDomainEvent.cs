namespace ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

public abstract record BaseDomainEvent
{
    // init 而非唯讀：Outbox 從資料庫還原事件時要能把原本的信封值寫回去，消費端才能用 EventId 去重。
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

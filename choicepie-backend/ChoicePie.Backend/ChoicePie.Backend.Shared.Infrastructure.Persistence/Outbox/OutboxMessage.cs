namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Outbox;

/// <summary>
/// 等待可靠派送的領域事件。這是基礎設施的佇列紀錄，不是領域實體：沒有稽核欄位、不做軟刪除，
/// 派送成功就整列刪掉（payload 可能含 raw token，不留存）。
/// </summary>
public sealed class OutboxMessage
{
    /// <summary>等於事件的 EventId，同一事件不會寫兩次。</summary>
    public Guid Id { get; private set; }

    public string Type { get; private set; } = null!;

    /// <summary>以 Data Protection 加密的事件 JSON。</summary>
    public string Payload { get; private set; } = null!;

    public DateTime OccurredOn { get; private set; }

    /// <summary>已成功處理過這筆事件的 handler；重試時跳過，只補跑失敗的那幾個。</summary>
    public List<string> CompletedHandlers { get; private set; } = [];

    public int AttemptCount { get; private set; }

    /// <summary>失敗後退避：這個時間點之前不會被認領。</summary>
    public DateTime NextAttemptAt { get; private set; }

    /// <summary>認領租約；處理中的節點掛掉時，租約到期別台才能接手。</summary>
    public DateTime? LockedUntil { get; private set; }

    public string? LastError { get; private set; }

    /// <summary>重試用盡；不再自動派送，留著給人查原因。</summary>
    public DateTime? DeadLetteredAt { get; private set; }

    private OutboxMessage()
    {
    }

    public void MarkHandlersCompleted(IEnumerable<string> handlers) =>
        CompletedHandlers = CompletedHandlers.Union(handlers).ToList();

    public static OutboxMessage Create(Guid eventId, string type, string payload, DateTime occurredOn) =>
        new() { Id = eventId, Type = type, Payload = payload, OccurredOn = occurredOn, NextAttemptAt = occurredOn };
}

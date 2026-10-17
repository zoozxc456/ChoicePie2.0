using Microsoft.EntityFrameworkCore;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Outbox;

public sealed class OutboxStore(DbContext context, TimeProvider timeProvider)
{
    public const int MaxAttempts = 10;

    private static readonly TimeSpan MaxBackoff = TimeSpan.FromHours(1);

    /// <summary>
    /// 認領一批到期的訊息並押上租約。FOR UPDATE SKIP LOCKED 讓多台同時認領時互不阻塞、也不會拿到同一筆；
    /// 租約讓認領後掛掉的節點不會把訊息永遠卡住。
    /// </summary>
    public async Task<IReadOnlyList<OutboxMessage>> ClaimBatchAsync(int batchSize, TimeSpan lease, CancellationToken ct)
    {
        var now = UtcNow();
        var lockedUntil = now.Add(lease);

        return await context.Set<OutboxMessage>()
            .FromSql($"""
                UPDATE outbox_messages SET locked_until = {lockedUntil}
                WHERE id IN (
                    SELECT id FROM outbox_messages
                    WHERE dead_lettered_at IS NULL
                      AND next_attempt_at <= {now}
                      AND (locked_until IS NULL OR locked_until < {now})
                    ORDER BY occurred_on
                    LIMIT {batchSize}
                    FOR UPDATE SKIP LOCKED)
                RETURNING *
                """)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public Task CompleteAsync(Guid id, CancellationToken ct) =>
        context.Set<OutboxMessage>().Where(m => m.Id == id).ExecuteDeleteAsync(ct);

    public async Task FailAsync(Guid id, IReadOnlyCollection<string> completedHandlers, string error, CancellationToken ct)
    {
        var message = await context.Set<OutboxMessage>().AsNoTracking().SingleAsync(m => m.Id == id, ct);
        message.MarkHandlersCompleted(completedHandlers);

        var attempt = message.AttemptCount + 1;
        var now = UtcNow();
        DateTime? deadLetteredAt = attempt >= MaxAttempts ? now : null;

        await context.Set<OutboxMessage>()
            .Where(m => m.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.AttemptCount, attempt)
                .SetProperty(m => m.NextAttemptAt, now.Add(BackoffFor(attempt)))
                .SetProperty(m => m.LockedUntil, (DateTime?)null)
                .SetProperty(m => m.LastError, error)
                .SetProperty(m => m.CompletedHandlers, message.CompletedHandlers)
                .SetProperty(m => m.DeadLetteredAt, deadLetteredAt), ct);
    }

    /// <summary>10 秒起跳、每次加倍，最多一小時；十次大約橫跨三小時，足以撐過 SMTP 之類的短暫中斷。</summary>
    public static TimeSpan BackoffFor(int attempt) =>
        TimeSpan.FromSeconds(Math.Min(10 * Math.Pow(2, attempt - 1), MaxBackoff.TotalSeconds));

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
}

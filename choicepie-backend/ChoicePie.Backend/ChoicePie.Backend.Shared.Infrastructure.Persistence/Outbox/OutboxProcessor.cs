using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Outbox;

/// <summary>
/// 每台後端都跑一個：輪詢 outbox、認領一批、逐筆派送。多台之間靠 <see cref="OutboxStore.ClaimBatchAsync"/>
/// 的 SKIP LOCKED + 租約分工，不需要選主。
/// </summary>
public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    OutboxDispatcher dispatcher,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private const int BatchSize = 20;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(1);

    // 要涵蓋一整批最慢的情況（寄信逾時等）；租約過期前沒處理完，別台可能接手造成重送。
    private static readonly TimeSpan Lease = TimeSpan.FromMinutes(5);

    public async Task<int> ProcessBatchAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<OutboxStore>();
        var batch = await store.ClaimBatchAsync(BatchSize, Lease, ct);

        foreach (var message in batch)
        {
            OutboxDispatchResult result;
            try
            {
                result = await dispatcher.DispatchAsync(message, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // 還原不了事件（型別被刪、金鑰遺失）也算失敗，最後會進 dead letter 而不是無限重試。
                result = new OutboxDispatchResult([], ex.Message);
            }

            if (result.Succeeded)
            {
                await store.CompleteAsync(message.Id, ct);
                continue;
            }

            await store.FailAsync(message.Id, result.CompletedHandlers, result.Error!, ct);
            if (message.AttemptCount + 1 >= OutboxStore.MaxAttempts)
            {
                logger.LogError("Outbox 訊息 {MessageId} ({Type}) 重試用盡，已轉為 dead letter：{Error}",
                    message.Id, message.Type, result.Error);
            }
            else
            {
                logger.LogWarning("Outbox 訊息 {MessageId} ({Type}) 第 {Attempt} 次派送失敗，稍後重試：{Error}",
                    message.Id, message.Type, message.AttemptCount + 1, result.Error);
            }
        }

        return batch.Count;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var processed = 0;
            try
            {
                processed = await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Outbox 輪詢失敗，稍後再試");
            }

            // 滿批代表可能還有積壓，馬上再拿下一批。
            if (processed < BatchSize)
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
        }
    }
}

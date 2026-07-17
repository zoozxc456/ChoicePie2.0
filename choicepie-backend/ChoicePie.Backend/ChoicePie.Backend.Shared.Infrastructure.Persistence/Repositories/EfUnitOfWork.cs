using System.Data;
using ChoicePie.Backend.Shared.Application.Events;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;

public class EfUnitOfWork<TContext>(TContext context, IPublisher mediator, IServiceScopeFactory scopeFactory)
    : IUnitOfWork where TContext : DbContext
{
    private IDbContextTransaction? _transaction;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var domainEvents = GetAndClearDomainEvents();
        FixPhantomModifiedOwnedEntities();

        var result = await context.SaveChangesAsync(ct);

        await DispatchDomainEventsAsync(domainEvents, ct);
        return result;
    }

    /// <summary>
    /// 聚合根的子實體（owned collection，例如 Quiz.Questions）主鍵是 Domain 層用 Guid.NewGuid() 提前生成的，
    /// 不是資料庫自動生成的遞增值。當一個「已追蹤聚合根」新增了這樣的子實體（例如 Quiz.AddQuestion），
    /// EF 在把它加進 owned collection 的當下（不是等到 SaveChanges 的 DetectChanges）就會立刻幫它建立
    /// tracking entry——但因為這個子實體從沒被 context.Add() 過，EF 只能憑「主鍵是不是 CLR 預設值」這條
    /// 慣例線索猜測狀態：Guid 主鍵永遠不是預設值 Guid.Empty，所以永遠被直接標成 Modified（誤判成「這筆
    /// 資料已經存在」），最終對一筆從沒 INSERT 過的 row 送出 UPDATE，炸出 DbUpdateConcurrencyException
    /// （expected 1 row, affected 0）。
    ///
    /// 這種「幽靈 Modified」有一個可靠的判斷特徵：因為 EF 從沒真的從資料庫載入過這筆資料的原始值，它把
    /// CurrentValues 整份拿去當 OriginalValues 用，所以連 CreatedAt 這種「建立時設一次、之後任何合法
    /// domain 方法都不會再動」的稽核欄位都會被回報成 IsModified=true。真正「載入後修改」的實體，即使
    /// State 也是 Modified，CreatedAt 依然是 IsModified=false（實測驗證過：Question.Update() 只會動
    /// Text/Choices/Explanation，不會動 CreatedAt）。用這個訊號把幽靈 Modified 導正成 Added，不需要
    /// 額外的快照比對，也不會誤傷真正的區域性更新。
    /// </summary>
    private void FixPhantomModifiedOwnedEntities()
    {
        context.ChangeTracker.DetectChanges();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Modified)
            {
                continue;
            }

            var createdAtProperty = entry.Properties
                .FirstOrDefault(p => p.Metadata.Name == nameof(IAuditableEntity.CreatedAt));

            if (createdAtProperty is { IsModified: true })
            {
                entry.State = EntityState.Added;
            }
        }
    }

    public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken ct = default)
    {
        _transaction ??= await context.Database.BeginTransactionAsync(isolationLevel, ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        try
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync(ct);
            }
        }
        finally
        {
            await ReleaseTransactionAsync();
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        try
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(ct);
            }
        }
        finally
        {
            await ReleaseTransactionAsync();
        }
    }

    private async Task ReleaseTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Domain Event Logic

    /// <summary>
    /// 從變更追蹤器中抓取所有領域事件，並將它們從實體中清除。
    /// </summary>
    private List<BaseDomainEvent> GetAndClearDomainEvents()
    {
        var entities = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        // 立即清除，防止重複分派
        entities.ForEach(e => e.Entity.ClearDomainEvents());

        return domainEvents;
    }

    private async Task DispatchDomainEventsAsync(IEnumerable<BaseDomainEvent> events, CancellationToken ct)
    {
        foreach (var @event in events)
        {
            var notification = CreateNotification((dynamic)@event);

            if (notification is not null)
            {
                await mediator.Publish(notification, ct);
            }
        }
    }

    #endregion

    private static DomainEventNotification<T> CreateNotification<T>(T @event) where T : BaseDomainEvent
        => new(@event);
}
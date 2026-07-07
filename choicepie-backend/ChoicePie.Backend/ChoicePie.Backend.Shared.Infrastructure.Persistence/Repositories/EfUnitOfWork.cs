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
        var result = await context.SaveChangesAsync(ct);

        await DispatchDomainEventsAsync(domainEvents, ct);
        return result;
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
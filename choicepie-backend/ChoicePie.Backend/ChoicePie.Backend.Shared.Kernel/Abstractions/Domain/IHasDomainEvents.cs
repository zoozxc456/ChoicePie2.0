namespace ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

public interface IHasDomainEvents
{
    /// <summary>
    /// 取得所有已引發但尚未分派的領域事件 (唯讀)。
    /// </summary>
    IReadOnlyCollection<BaseDomainEvent> DomainEvents { get; }

    /// <summary>
    /// 清除所有已引發的領域事件。
    /// (由 UnitOfWork 在分派完成後呼叫)
    /// </summary>
    void ClearDomainEvents();
}
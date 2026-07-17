using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests.TestSupport;

/// <summary>
/// EfGenericRepository/EfUnitOfWork/SpecificationEvaluator 是跨聚合共用的泛型基礎設施，測這些東西不需要
/// 依賴任何一個真實的 product aggregate（那樣反而會讓 Shared.Infrastructure.Persistence 這個底層專案
/// 反向依賴上層 Domain，違反 Clean Architecture 依賴方向）——用一個最小的假聚合根就夠了。
/// </summary>
public sealed class TestWidget : AggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;

    private TestWidget()
    {
    }

    public static TestWidget Create(string name)
    {
        var widget = new TestWidget
        {
            Id = Guid.NewGuid(),
            Name = name
        };

        widget.SetCreated(widget.Id);
        widget.AddDomainEvent(new TestWidgetCreatedDomainEvent(widget.Id, name));

        return widget;
    }

    public void Rename(string name)
    {
        Name = name;
    }
}

public sealed record TestWidgetCreatedDomainEvent(Guid WidgetId, string Name) : BaseDomainEvent;

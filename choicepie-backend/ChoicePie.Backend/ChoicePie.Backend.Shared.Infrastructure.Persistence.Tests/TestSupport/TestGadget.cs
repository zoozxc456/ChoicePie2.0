using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests.TestSupport;

/// <summary>測衝突轉換用：Code 有唯一索引，整筆資料以 xmin 做樂觀並行控制。</summary>
public sealed class TestGadget : AggregateRoot<Guid>
{
    public string Code { get; private set; } = null!;
    public string Label { get; private set; } = null!;

    private TestGadget()
    {
    }

    public static TestGadget Create(string code)
    {
        var gadget = new TestGadget { Id = Guid.NewGuid(), Code = code, Label = code };
        gadget.SetCreated(gadget.Id);
        return gadget;
    }

    public void Relabel(string label)
    {
        Label = label;
    }
}

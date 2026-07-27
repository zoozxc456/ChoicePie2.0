using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Shared.Kernel.Tests.Primitives;

public sealed class TestAppendOnlyRecord : AppendOnlyAuditableEntity<Guid>
{
    public static TestAppendOnlyRecord Create(Guid creatorId)
    {
        var record = new TestAppendOnlyRecord { Id = Guid.NewGuid() };
        record.MarkCreated(creatorId);
        return record;
    }

    private void MarkCreated(Guid creatorId) => SetCreated(creatorId);
}

[TestFixture]
public class AppendOnlyAuditableEntityTests
{
    [Test]
    public void Create_GivenCreatorId_WhenCalled_ThenSetsCreatorIdAndCreatedAt()
    {
        var creatorId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        var record = TestAppendOnlyRecord.Create(creatorId);

        Assert.That(record.CreatorId, Is.EqualTo(creatorId));
        Assert.That(record.CreatedAt, Is.GreaterThanOrEqualTo(before));
    }
}

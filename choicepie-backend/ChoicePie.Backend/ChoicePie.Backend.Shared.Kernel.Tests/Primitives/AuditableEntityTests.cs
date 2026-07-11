using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Shared.Kernel.Tests.Primitives;

[TestFixture]
public class AuditableEntityTests
{
    private sealed class TestAuditableEntity : AuditableEntity<Guid>;

    [Test]
    public void IsDeleted_GivenFreshEntity_WhenNeverDeleted_ThenReturnsFalse()
    {
        var entity = new TestAuditableEntity();

        Assert.That(entity.IsDeleted, Is.False);
    }

    [Test]
    public void IsDeleted_GivenEntity_WhenDeleted_ThenReturnsTrue()
    {
        var entity = new TestAuditableEntity();

        entity.Delete();

        Assert.That(entity.IsDeleted, Is.True);
    }
}

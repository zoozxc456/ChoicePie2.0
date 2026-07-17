using ChoicePie.Backend.Shared.Application.Contracts;

namespace ChoicePie.Backend.Shared.Application.Tests.Contracts;

[TestFixture]
public class BaseResultTests
{
    [Test]
    public void Items_GivenConstructedWithItems_WhenRead_ThenReturnsTheSameItems()
    {
        var result = new BaseResult<int>([1, 2, 3]);

        Assert.That(result.Items, Is.EquivalentTo(new[] { 1, 2, 3 }));
    }
}

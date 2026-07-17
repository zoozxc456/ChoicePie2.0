using ChoicePie.Backend.Shared.Application.Contracts;

namespace ChoicePie.Backend.Shared.Application.Tests.Contracts;

[TestFixture]
public class PagedResultTests
{
    [TestCase(0, 10, 0)]
    [TestCase(1, 10, 1)]
    [TestCase(10, 10, 1)]
    [TestCase(11, 10, 2)]
    [TestCase(25, 10, 3)]
    public void TotalPages_GivenTotalCountAndPageSize_WhenCalculated_ThenRoundsUpToNearestWholePage(
        int totalCount, int pageSize, int expectedTotalPages)
    {
        var result = new PagedResult<string>([], 1, pageSize, totalCount);

        Assert.That(result.TotalPages, Is.EqualTo(expectedTotalPages));
    }

    [Test]
    public void TotalPages_GivenPageSizeOfZero_WhenCalculated_ThenDoesNotDivideByZero()
    {
        var result = new PagedResult<string>([], 1, 0, 10);

        Assert.That(() => result.TotalPages, Throws.Nothing);
    }

    [Test]
    public void HasNextPage_GivenCurrentPageBeforeLastPage_WhenCalculated_ThenReturnsTrue()
    {
        var result = new PagedResult<string>([], 1, 10, 25);

        Assert.That(result.HasNextPage, Is.True);
    }

    [Test]
    public void HasNextPage_GivenCurrentPageIsLastPage_WhenCalculated_ThenReturnsFalse()
    {
        var result = new PagedResult<string>([], 3, 10, 25);

        Assert.That(result.HasNextPage, Is.False);
    }

    [Test]
    public void Items_GivenConstructedWithItems_WhenRead_ThenReturnsTheSameItems()
    {
        var result = new PagedResult<string>(["a", "b"], 1, 10, 2);

        Assert.That(result.Items, Is.EquivalentTo(new[] { "a", "b" }));
    }
}

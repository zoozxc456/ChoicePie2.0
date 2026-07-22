using ChoicePie.Backend.Shared.Application.Contracts;

namespace ChoicePie.Backend.Shared.Application.Tests.Contracts;

[TestFixture]
public class PaginationParametersTests
{
    private sealed class TestPaginationParameters : PaginationParameters;

    [Test]
    public void PageSize_GivenValueWithinRange_WhenSet_ThenReturnsThatValue()
    {
        var parameters = new TestPaginationParameters { PageSize = 50 };

        Assert.That(parameters.PageSize, Is.EqualTo(50));
    }

    [Test]
    public void PageSize_GivenNoValueSet_WhenRead_ThenDefaultsTo20()
    {
        var parameters = new TestPaginationParameters();

        Assert.That(parameters.PageSize, Is.EqualTo(20));
    }

    [Test]
    public void PageSize_GivenValueAboveMax_WhenSet_ThenClampsTo100()
    {
        var parameters = new TestPaginationParameters { PageSize = 500 };

        Assert.That(parameters.PageSize, Is.EqualTo(100));
    }

    [Test]
    public void PageSize_GivenValueOfZeroOrLess_WhenSet_ThenKeepsCurrentValue()
    {
        var parameters = new TestPaginationParameters { PageSize = 50 };

        parameters.PageSize = 0;

        Assert.That(parameters.PageSize, Is.EqualTo(50));
    }
}

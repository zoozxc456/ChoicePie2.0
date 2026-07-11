using ChoicePie.Backend.Shared.Kernel.ValueObjects;

namespace ChoicePie.Backend.Shared.Kernel.Tests.ValueObjects;

[TestFixture]
public class PersonNameTests
{
    private static Exception InvalidNameException(string name) => new InvalidOperationException(name);

    [Test]
    public void Create_GivenValidName_WhenCalled_ThenReturnsPersonNameWithValue()
    {
        var personName = PersonName.Create("Host Name", InvalidNameException);

        Assert.That(personName.Value, Is.EqualTo("Host Name"));
    }

    [Test]
    public void Create_GivenNameTooShort_WhenCalled_ThenThrowsExceptionFromFactory()
    {
        Assert.Throws<InvalidOperationException>(() => PersonName.Create("a", InvalidNameException));
    }

    [Test]
    public void Create_GivenNameTooLong_WhenCalled_ThenThrowsExceptionFromFactory()
    {
        Assert.Throws<InvalidOperationException>(
            () => PersonName.Create("this-name-is-way-too-long-for-a-person", InvalidNameException));
    }

    [Test]
    public void Create_GivenBlankName_WhenCalled_ThenThrowsExceptionFromFactory()
    {
        Assert.Throws<InvalidOperationException>(() => PersonName.Create("  ", InvalidNameException));
    }
}

using ChoicePie.Backend.Shared.Kernel.Exceptions;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;

namespace ChoicePie.Backend.Shared.Kernel.Tests.ValueObjects;

[TestFixture]
public class EmailTests
{
    [Test]
    public void Create_GivenValidEmail_WhenCalled_ThenReturnsTrimmedLowercasedValue()
    {
        var email = Email.Create("  Test@Example.COM  ");

        Assert.That(email.Value, Is.EqualTo("test@example.com"));
    }

    [TestCase("not-an-email")]
    [TestCase("missing-domain@")]
    [TestCase("@missing-local.com")]
    [TestCase("no-at-sign.com")]
    [TestCase("   ")]
    public void Create_GivenInvalidFormat_WhenCalled_ThenThrowsInvalidEmailException(string value)
    {
        Assert.Throws<InvalidEmailException>(() => Email.Create(value));
    }

    [Test]
    public void Equals_GivenTwoEmailsWithSameValue_WhenCompared_ThenAreEqual()
    {
        var first = Email.Create("same@example.com");
        var second = Email.Create("Same@Example.com");

        Assert.That(first, Is.EqualTo(second));
    }
}

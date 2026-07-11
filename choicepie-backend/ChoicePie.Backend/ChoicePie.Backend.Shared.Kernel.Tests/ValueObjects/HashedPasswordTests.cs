using ChoicePie.Backend.Shared.Kernel.Exceptions;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;

namespace ChoicePie.Backend.Shared.Kernel.Tests.ValueObjects;

[TestFixture]
public class HashedPasswordTests
{
    [Test]
    public void Create_GivenValidHashAndSalt_WhenCalled_ThenReturnsHashedPassword()
    {
        var hashedPassword = HashedPassword.Create("hashed-value", "salt-value");

        Assert.Multiple(() =>
        {
            Assert.That(hashedPassword.Hash, Is.EqualTo("hashed-value"));
            Assert.That(hashedPassword.Salt, Is.EqualTo("salt-value"));
        });
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Create_GivenBlankHash_WhenCalled_ThenThrowsInvalidHashedPasswordException(string hash)
    {
        Assert.Throws<InvalidHashedPasswordException>(() => HashedPassword.Create(hash, "salt-value"));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Create_GivenBlankSalt_WhenCalled_ThenThrowsInvalidHashedPasswordException(string salt)
    {
        Assert.Throws<InvalidHashedPasswordException>(() => HashedPassword.Create("hashed-value", salt));
    }

    [Test]
    public void Equals_GivenTwoInstancesWithSameHashAndSalt_WhenCompared_ThenAreEqual()
    {
        var first = HashedPassword.Create("hashed-value", "salt-value");
        var second = HashedPassword.Create("hashed-value", "salt-value");

        Assert.That(first, Is.EqualTo(second));
    }
}

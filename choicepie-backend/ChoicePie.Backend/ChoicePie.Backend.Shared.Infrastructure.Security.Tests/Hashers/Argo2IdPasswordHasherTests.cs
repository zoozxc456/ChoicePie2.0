using ChoicePie.Backend.Shared.Infrastructure.Security.Hashers;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;

namespace ChoicePie.Backend.Shared.Infrastructure.Security.Tests.Hashers;

[TestFixture]
public class Argo2IdPasswordHasherTests
{
    private Argo2IdPasswordHasher _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new Argo2IdPasswordHasher();
    }

    [Test]
    public void Hash_GivenPassword_WhenCalled_ThenReturnsNonEmptyHashAndSalt()
    {
        var result = _sut.Hash("Password123!");

        Assert.That(result.Hash, Is.Not.Null.And.Not.Empty);
        Assert.That(result.Salt, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void Hash_GivenSamePasswordTwice_WhenCalled_ThenProducesDifferentHashesDueToRandomSalt()
    {
        var first = _sut.Hash("Password123!");
        var second = _sut.Hash("Password123!");

        Assert.That(first.Salt, Is.Not.EqualTo(second.Salt));
        Assert.That(first.Hash, Is.Not.EqualTo(second.Hash));
    }

    [Test]
    public void Verify_GivenCorrectPassword_WhenCalled_ThenReturnsTrue()
    {
        var hashed = _sut.Hash("Password123!");

        var result = _sut.Verify("Password123!", hashed);

        Assert.That(result, Is.True);
    }

    [Test]
    public void Verify_GivenWrongPassword_WhenCalled_ThenReturnsFalse()
    {
        var hashed = _sut.Hash("Password123!");

        var result = _sut.Verify("WrongPassword123!", hashed);

        Assert.That(result, Is.False);
    }

    [Test]
    public void Verify_GivenTamperedHash_WhenCalled_ThenReturnsFalse()
    {
        var hashed = _sut.Hash("Password123!");
        var tampered = HashedPassword.Create(hashed.Hash + "x", hashed.Salt);

        var result = _sut.Verify("Password123!", tampered);

        Assert.That(result, Is.False);
    }
}

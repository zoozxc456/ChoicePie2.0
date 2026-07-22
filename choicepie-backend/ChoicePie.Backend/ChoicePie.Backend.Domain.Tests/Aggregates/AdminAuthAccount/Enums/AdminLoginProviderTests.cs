using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Enums;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.AdminAuthAccount.Enums;

[TestFixture]
public class AdminLoginProviderTests
{
    [Test]
    public void FromName_GivenKnownNameDifferentCase_WhenCalled_ThenReturnsMatchingInstance()
    {
        var provider = AdminLoginProvider.FromName("original");

        Assert.That(provider, Is.EqualTo(AdminLoginProvider.Original));
    }

    [Test]
    public void FromName_GivenUnknownName_WhenCalled_ThenReturnsNull()
    {
        Assert.That(AdminLoginProvider.FromName("ldap"), Is.Null);
    }

    [Test]
    public void FromValue_GivenKnownId_WhenCalled_ThenReturnsMatchingInstance()
    {
        Assert.That(AdminLoginProvider.FromValue(1), Is.EqualTo(AdminLoginProvider.Original));
    }

    [Test]
    public void Enumerations_WhenRead_ThenContainsExactlyOneProvider()
    {
        Assert.That(AdminLoginProvider.Enumerations.Values, Is.EquivalentTo(new[] { AdminLoginProvider.Original }));
    }
}

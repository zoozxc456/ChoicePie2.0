using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.AuthAccount.Enums;

[TestFixture]
public class LoginProviderTests
{
    [Test]
    public void FromName_GivenKnownNameDifferentCase_WhenCalled_ThenReturnsMatchingInstance()
    {
        var provider = LoginProvider.FromName("google");

        Assert.That(provider, Is.EqualTo(LoginProvider.Google));
    }

    [Test]
    public void FromName_GivenUnknownName_WhenCalled_ThenReturnsNull()
    {
        Assert.That(LoginProvider.FromName("twitter"), Is.Null);
    }

    [Test]
    public void FromValue_GivenKnownId_WhenCalled_ThenReturnsMatchingInstance()
    {
        Assert.That(LoginProvider.FromValue(1), Is.EqualTo(LoginProvider.Original));
    }

    [Test]
    public void Enumerations_WhenRead_ThenContainsExactlyFourProviders()
    {
        Assert.That(LoginProvider.Enumerations.Values, Is.EquivalentTo(new[]
        {
            LoginProvider.Original, LoginProvider.Google, LoginProvider.Meta, LoginProvider.Line
        }));
    }
}

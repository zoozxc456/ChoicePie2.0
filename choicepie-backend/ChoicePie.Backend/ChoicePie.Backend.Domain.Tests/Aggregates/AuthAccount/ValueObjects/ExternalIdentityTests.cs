using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.ValueObjects;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.AuthAccount.ValueObjects;

[TestFixture]
public class ExternalIdentityTests
{
    [Test]
    public void Create_GivenProviderAndProviderUserId_WhenCalled_ThenReturnsExternalIdentityWithExpectedFields()
    {
        var identity = ExternalIdentity.Create(LoginProvider.Google, "google-subject-id");

        Assert.Multiple(() =>
        {
            Assert.That(identity.Provider, Is.EqualTo(LoginProvider.Google));
            Assert.That(identity.ProviderUserId, Is.EqualTo("google-subject-id"));
        });
    }
}

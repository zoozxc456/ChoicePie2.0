using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Enums;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.ValueObjects;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.AdminAuthAccount.ValueObjects;

[TestFixture]
public class AdminExternalIdentityTests
{
    [Test]
    public void Create_GivenProviderAndProviderUserId_WhenCalled_ThenReturnsAdminExternalIdentityWithExpectedFields()
    {
        var identity = AdminExternalIdentity.Create(AdminLoginProvider.Original, "external-subject-id");

        Assert.Multiple(() =>
        {
            Assert.That(identity.Provider, Is.EqualTo(AdminLoginProvider.Original));
            Assert.That(identity.ProviderUserId, Is.EqualTo("external-subject-id"));
        });
    }
}

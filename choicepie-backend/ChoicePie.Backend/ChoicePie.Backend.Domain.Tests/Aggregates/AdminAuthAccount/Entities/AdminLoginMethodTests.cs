using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Entities;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Enums;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.AdminAuthAccount.Entities;

[TestFixture]
public class AdminLoginMethodTests
{
    [Test]
    public void CreateOriginal_GivenPasswordHash_WhenCalled_ThenReturnsOriginalLoginMethod()
    {
        var loginMethod = AdminLoginMethod.CreateOriginal("hashed-password", "salt");

        Assert.Multiple(() =>
        {
            Assert.That(loginMethod.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(loginMethod.Provider, Is.EqualTo(AdminLoginProvider.Original));
            Assert.That(loginMethod.PasswordHash, Is.EqualTo("hashed-password"));
            Assert.That(loginMethod.ProviderUserId, Is.Null);
        });
    }

    [Test]
    public void CreateExternal_GivenProviderAndProviderUserId_WhenCalled_ThenReturnsExternalLoginMethod()
    {
        var loginMethod = AdminLoginMethod.CreateExternal(AdminLoginProvider.Original, "external-subject-id");

        Assert.Multiple(() =>
        {
            Assert.That(loginMethod.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(loginMethod.Provider, Is.EqualTo(AdminLoginProvider.Original));
            Assert.That(loginMethod.ProviderUserId, Is.EqualTo("external-subject-id"));
            Assert.That(loginMethod.PasswordHash, Is.Null);
        });
    }
}
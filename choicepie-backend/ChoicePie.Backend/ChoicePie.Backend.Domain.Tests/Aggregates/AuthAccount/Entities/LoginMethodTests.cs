using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Entities;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.AuthAccount.Entities;

[TestFixture]
public class LoginMethodTests
{
    [Test]
    public void CreateOriginal_GivenPasswordHash_WhenCalled_ThenReturnsOriginalLoginMethod()
    {
        var loginMethod = LoginMethod.CreateOriginal("hashed-password", "salt");

        Assert.Multiple(() =>
        {
            Assert.That(loginMethod.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(loginMethod.Provider, Is.EqualTo(LoginProvider.Original));
            Assert.That(loginMethod.PasswordHash, Is.EqualTo("hashed-password"));
            Assert.That(loginMethod.ProviderUserId, Is.Null);
        });
    }

    [Test]
    public void CreateExternal_GivenProviderAndProviderUserId_WhenCalled_ThenReturnsExternalLoginMethod()
    {
        var loginMethod = LoginMethod.CreateExternal(LoginProvider.Google, "google-subject-id");

        Assert.Multiple(() =>
        {
            Assert.That(loginMethod.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(loginMethod.Provider, Is.EqualTo(LoginProvider.Google));
            Assert.That(loginMethod.ProviderUserId, Is.EqualTo("google-subject-id"));
            Assert.That(loginMethod.PasswordHash, Is.Null);
        });
    }
}
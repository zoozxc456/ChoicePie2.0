using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using AuthAccountAggregate = ChoicePie.Backend.Domain.Aggregates.AuthAccount.AuthAccount;
using LoginProvider = ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums.LoginProvider;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.AuthAccount;

[TestFixture]
public class AuthAccountSettingsTests
{
    private static readonly Guid MemberId = Guid.NewGuid();

    private static AuthAccountAggregate CreatePasswordAccount() =>
        AuthAccountAggregate.Register(
            Email.Create("host@example.com"), HashedPassword.Create("hashed-password", "salt"), MemberId);

    private static AuthAccountAggregate CreateGoogleOnlyAccount() =>
        AuthAccountAggregate.RegisterWithExternalLogin(
            Email.Create("host@example.com"), LoginProvider.Google, "google-subject", MemberId);

    [Test]
    public void HasPassword_GivenPasswordAccount_WhenRead_ThenReturnsTrue()
    {
        Assert.That(CreatePasswordAccount().HasPassword, Is.True);
    }

    [Test]
    public void HasPassword_GivenGoogleOnlyAccount_WhenRead_ThenReturnsFalse()
    {
        Assert.That(CreateGoogleOnlyAccount().HasPassword, Is.False);
    }

    [Test]
    public void IsLinkedTo_GivenGoogleLinkedAccount_WhenAskedAboutGoogle_ThenReturnsTrue()
    {
        Assert.Multiple(() =>
        {
            Assert.That(CreateGoogleOnlyAccount().IsLinkedTo(LoginProvider.Google), Is.True);
            Assert.That(CreatePasswordAccount().IsLinkedTo(LoginProvider.Google), Is.False);
        });
    }

    [Test]
    public void AddPassword_GivenGoogleOnlyAccount_WhenCalled_ThenAddsOriginalLoginMethod()
    {
        var authAccount = CreateGoogleOnlyAccount();

        authAccount.AddPassword(HashedPassword.Create("new-hash", "new-salt"));

        Assert.Multiple(() =>
        {
            Assert.That(authAccount.HasPassword, Is.True);
            Assert.That(authAccount.OriginalPassword, Is.EqualTo(HashedPassword.Create("new-hash", "new-salt")));
            Assert.That(authAccount.LoginMethods, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void AddPassword_GivenAccountWithPassword_WhenCalled_ThenThrowsPasswordAlreadySetException()
    {
        var authAccount = CreatePasswordAccount();

        Assert.Throws<PasswordAlreadySetException>(
            () => authAccount.AddPassword(HashedPassword.Create("new-hash", "new-salt")));
    }

    [Test]
    public void RemoveLoginMethod_GivenGoogleAndPassword_WhenRemovingGoogle_ThenKeepsOnlyPassword()
    {
        var authAccount = CreatePasswordAccount();
        authAccount.AddLoginMethod(LoginProvider.Google, "google-subject");

        authAccount.RemoveLoginMethod(LoginProvider.Google);

        Assert.Multiple(() =>
        {
            Assert.That(authAccount.IsLinkedTo(LoginProvider.Google), Is.False);
            Assert.That(authAccount.LoginMethods, Has.Count.EqualTo(1));
            Assert.That(authAccount.HasPassword, Is.True);
        });
    }

    [Test]
    public void RemoveLoginMethod_GivenGoogleIsTheOnlyMethod_WhenRemovingGoogle_ThenThrowsCannotRemoveLastLoginMethodException()
    {
        var authAccount = CreateGoogleOnlyAccount();

        Assert.Throws<CannotRemoveLastLoginMethodException>(() => authAccount.RemoveLoginMethod(LoginProvider.Google));
    }

    [Test]
    public void RemoveLoginMethod_GivenProviderNotLinked_WhenCalled_ThenThrowsLoginMethodNotLinkedException()
    {
        var authAccount = CreatePasswordAccount();

        Assert.Throws<LoginMethodNotLinkedException>(() => authAccount.RemoveLoginMethod(LoginProvider.Google));
    }

    [Test]
    public void RemoveLoginMethod_GivenOriginalProvider_WhenCalled_ThenThrowsLoginMethodNotLinkedException()
    {
        // 密碼不能透過「解除連結」移除，只能被取代。
        var authAccount = CreatePasswordAccount();
        authAccount.AddLoginMethod(LoginProvider.Google, "google-subject");

        Assert.Throws<LoginMethodNotLinkedException>(() => authAccount.RemoveLoginMethod(LoginProvider.Original));
    }

    [Test]
    public void MarkDeleted_GivenActiveAccount_WhenCalled_ThenSoftDeletesAndReleasesEmailAndCredentials()
    {
        var authAccount = CreatePasswordAccount();
        authAccount.AddLoginMethod(LoginProvider.Google, "google-subject");

        authAccount.MarkDeleted(MemberId);

        Assert.Multiple(() =>
        {
            Assert.That(authAccount.IsDeleted, Is.True);
            Assert.That(authAccount.DeleterId, Is.EqualTo(MemberId));
            Assert.That(authAccount.Email.Value, Is.Not.EqualTo("host@example.com"));
            Assert.That(authAccount.Email.Value, Does.EndWith("@deleted.invalid"));
            Assert.That(authAccount.LoginMethods, Is.Empty);
        });
    }
}

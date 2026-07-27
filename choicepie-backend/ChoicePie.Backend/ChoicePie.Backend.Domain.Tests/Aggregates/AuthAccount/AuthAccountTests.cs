using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Events;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using AuthAccountAggregate = ChoicePie.Backend.Domain.Aggregates.AuthAccount.AuthAccount;
using LoginProvider = ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums.LoginProvider;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.AuthAccount;

[TestFixture]
public class AuthAccountTests
{
    private static readonly Guid MemberId = Guid.NewGuid();

    private static AuthAccountAggregate CreateAuthAccount() =>
        AuthAccountAggregate.Register(
            Email.Create("host@example.com"), HashedPassword.Create("hashed-password", "salt"), MemberId);

    [Test]
    public void Register_GivenValidInput_WhenCalled_ThenCreatesAuthAccountWithExpectedFields()
    {
        var authAccount = CreateAuthAccount();

        Assert.Multiple(() =>
        {
            Assert.That(authAccount.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(authAccount.Email, Is.EqualTo(Email.Create("host@example.com")));
            Assert.That(authAccount.MemberId, Is.EqualTo(MemberId));
            Assert.That(authAccount.IsVerified, Is.False);
            Assert.That(authAccount.LoginMethods, Has.Count.EqualTo(1));
            Assert.That(authAccount.LoginMethods[0].Provider, Is.EqualTo(LoginProvider.Original));
        });
    }

    [Test]
    public void Register_GivenValidInput_WhenCalled_ThenRaisesAuthAccountRegisteredDomainEvent()
    {
        var authAccount = CreateAuthAccount();

        var domainEvent = authAccount.DomainEvents.OfType<AuthAccountRegisteredDomainEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(domainEvent.AuthAccountId, Is.EqualTo(authAccount.Id));
            Assert.That(domainEvent.MemberId, Is.EqualTo(MemberId));
            Assert.That(domainEvent.Email, Is.EqualTo("host@example.com"));
        });
    }

    [Test]
    public void OriginalPassword_GivenRegisteredAccount_WhenRead_ThenReturnsHashedPassword()
    {
        var authAccount = CreateAuthAccount();

        Assert.That(authAccount.OriginalPassword, Is.EqualTo(HashedPassword.Create("hashed-password", "salt")));
    }

    [Test]
    public void AddLoginMethod_GivenNewProvider_WhenCalled_ThenAddsLoginMethod()
    {
        var authAccount = CreateAuthAccount();

        authAccount.AddLoginMethod(LoginProvider.Google, "google-subject-id");

        Assert.Multiple(() =>
        {
            Assert.That(authAccount.LoginMethods, Has.Count.EqualTo(2));
            Assert.That(authAccount.LoginMethods.Single(m => m.Provider == LoginProvider.Google).ProviderUserId,
                Is.EqualTo("google-subject-id"));
        });
    }

    [Test]
    public void AddLoginMethod_GivenAlreadyLinkedProvider_WhenCalled_ThenThrowsLoginMethodAlreadyLinkedException()
    {
        var authAccount = CreateAuthAccount();

        Assert.Throws<LoginMethodAlreadyLinkedException>(() =>
            authAccount.AddLoginMethod(LoginProvider.Original, "irrelevant"));
    }

    [Test]
    public void ChangePassword_GivenAccountWithOriginalLoginMethod_WhenCalled_ThenUpdatesPassword()
    {
        var authAccount = CreateAuthAccount();
        var newPassword = HashedPassword.Create("new-hash", "new-salt");

        authAccount.ChangePassword(newPassword);

        Assert.That(authAccount.OriginalPassword, Is.EqualTo(newPassword));
    }

    [Test]
    public void Verify_GivenUnverifiedAccount_WhenCalled_ThenSetsIsVerifiedTrue()
    {
        var authAccount = CreateAuthAccount();

        authAccount.Verify();

        Assert.That(authAccount.IsVerified, Is.True);
    }

    [Test]
    public void Verify_GivenAlreadyVerifiedAccount_WhenCalledAgain_ThenRemainsVerified()
    {
        var authAccount = CreateAuthAccount();
        authAccount.Verify();

        authAccount.Verify();

        Assert.That(authAccount.IsVerified, Is.True);
    }

    [Test]
    public void RegisterWithExternalLogin_GivenValidInput_WhenCalled_ThenCreatesVerifiedAccountWithExternalLoginMethod()
    {
        var authAccount = AuthAccountAggregate.RegisterWithExternalLogin(
            Email.Create("google-user@example.com"), LoginProvider.Google, "google-subject-id", MemberId);

        Assert.Multiple(() =>
        {
            Assert.That(authAccount.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(authAccount.Email, Is.EqualTo(Email.Create("google-user@example.com")));
            Assert.That(authAccount.MemberId, Is.EqualTo(MemberId));
            Assert.That(authAccount.IsVerified, Is.True);
            Assert.That(authAccount.LoginMethods, Has.Count.EqualTo(1));
            Assert.That(authAccount.LoginMethods[0].Provider, Is.EqualTo(LoginProvider.Google));
            Assert.That(authAccount.LoginMethods[0].ProviderUserId, Is.EqualTo("google-subject-id"));
            Assert.That(authAccount.OriginalPassword, Is.Null);
        });
    }

    [Test]
    public void RegisterWithExternalLogin_GivenValidInput_WhenCalled_ThenRaisesAuthAccountRegisteredDomainEvent()
    {
        var authAccount = AuthAccountAggregate.RegisterWithExternalLogin(
            Email.Create("google-user@example.com"), LoginProvider.Google, "google-subject-id", MemberId);

        var domainEvent = authAccount.DomainEvents.OfType<AuthAccountRegisteredDomainEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(domainEvent.AuthAccountId, Is.EqualTo(authAccount.Id));
            Assert.That(domainEvent.MemberId, Is.EqualTo(MemberId));
            Assert.That(domainEvent.Email, Is.EqualTo("google-user@example.com"));
        });
    }
}
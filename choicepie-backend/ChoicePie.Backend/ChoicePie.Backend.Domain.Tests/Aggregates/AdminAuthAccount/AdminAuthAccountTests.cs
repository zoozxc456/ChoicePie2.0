using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Enums;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Events;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Exceptions;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using AdminAuthAccountAggregate = ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.AdminAuthAccount;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.AdminAuthAccount;

[TestFixture]
public class AdminAuthAccountTests
{
    private static readonly Guid AdminUserId = Guid.NewGuid();

    private static AdminAuthAccountAggregate CreateAdminAuthAccount() =>
        AdminAuthAccountAggregate.Create(Email.Create("admin@example.com"), "hashed-password", AdminUserId);

    [Test]
    public void Create_GivenValidInput_WhenCalled_ThenCreatesAdminAuthAccountWithExpectedFields()
    {
        var adminAuthAccount = CreateAdminAuthAccount();

        Assert.Multiple(() =>
        {
            Assert.That(adminAuthAccount.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(adminAuthAccount.Email, Is.EqualTo(Email.Create("admin@example.com")));
            Assert.That(adminAuthAccount.AdminUserId, Is.EqualTo(AdminUserId));
            Assert.That(adminAuthAccount.IsVerified, Is.False);
            Assert.That(adminAuthAccount.LoginMethods, Has.Count.EqualTo(1));
            Assert.That(adminAuthAccount.LoginMethods[0].Provider, Is.EqualTo(AdminLoginProvider.Original));
        });
    }

    [Test]
    public void Create_GivenValidInput_WhenCalled_ThenRaisesAdminAuthAccountCreatedDomainEvent()
    {
        var adminAuthAccount = CreateAdminAuthAccount();

        var domainEvent = adminAuthAccount.DomainEvents.OfType<AdminAuthAccountCreatedDomainEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(domainEvent.AdminAuthAccountId, Is.EqualTo(adminAuthAccount.Id));
            Assert.That(domainEvent.AdminUserId, Is.EqualTo(AdminUserId));
            Assert.That(domainEvent.Email, Is.EqualTo("admin@example.com"));
        });
    }

    [Test]
    public void OriginalPasswordHash_GivenCreatedAccount_WhenRead_ThenReturnsPasswordHash()
    {
        var adminAuthAccount = CreateAdminAuthAccount();

        Assert.That(adminAuthAccount.OriginalPasswordHash, Is.EqualTo("hashed-password"));
    }

    [Test]
    public void AddLoginMethod_GivenAlreadyLinkedProvider_WhenCalled_ThenThrowsAdminLoginMethodAlreadyLinkedException()
    {
        var adminAuthAccount = CreateAdminAuthAccount();

        Assert.Throws<AdminLoginMethodAlreadyLinkedException>(() =>
            adminAuthAccount.AddLoginMethod(AdminLoginProvider.Original, "irrelevant"));
    }
}

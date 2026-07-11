using ChoicePie.Backend.Domain.Aggregates.AdminUser.Enums;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Events;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;
using AdminUserAggregate = ChoicePie.Backend.Domain.Aggregates.AdminUser.AdminUser;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.AdminUser;

[TestFixture]
public class AdminUserTests
{
    [Test]
    public void Create_GivenValidInput_WhenCalled_ThenCreatesAdminUserWithExpectedFields()
    {
        var adminUser = AdminUserAggregate.Create("Ops Name", AdminRole.Staff);

        Assert.Multiple(() =>
        {
            Assert.That(adminUser.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(adminUser.Name, Is.EqualTo("Ops Name"));
            Assert.That(adminUser.Role, Is.EqualTo(AdminRole.Staff));
        });
    }

    [Test]
    public void Create_GivenValidInput_WhenCalled_ThenRaisesAdminUserCreatedDomainEvent()
    {
        var adminUser = AdminUserAggregate.Create("Ops Name", AdminRole.Admin);

        var domainEvent = adminUser.DomainEvents.OfType<AdminUserCreatedDomainEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(domainEvent.AdminUserId, Is.EqualTo(adminUser.Id));
            Assert.That(domainEvent.Name, Is.EqualTo("Ops Name"));
            Assert.That(domainEvent.Role, Is.EqualTo("admin"));
        });
    }

    [TestCase("a")]
    [TestCase("this-name-is-way-too-long-for-an-admin-user")]
    [TestCase("  ")]
    public void Create_GivenNameOutOfRange_WhenCalled_ThenThrowsInvalidAdminUserNameException(string name)
    {
        Assert.Throws<InvalidAdminUserNameException>(() => AdminUserAggregate.Create(name, AdminRole.Staff));
    }
}

using ChoicePie.Backend.Domain.Aggregates.AdminUser.Enums;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.AdminUser.Enums;

[TestFixture]
public class AdminRoleTests
{
    [Test]
    public void FromName_GivenKnownNameDifferentCase_WhenCalled_ThenReturnsMatchingInstance()
    {
        var role = AdminRole.FromName("staff");

        Assert.That(role, Is.EqualTo(AdminRole.Staff));
    }

    [Test]
    public void FromName_GivenUnknownName_WhenCalled_ThenReturnsNull()
    {
        Assert.That(AdminRole.FromName("guest"), Is.Null);
    }

    [Test]
    public void FromValue_GivenKnownId_WhenCalled_ThenReturnsMatchingInstance()
    {
        Assert.That(AdminRole.FromValue(4), Is.EqualTo(AdminRole.SystemAdmin));
    }

    [Test]
    public void Enumerations_WhenRead_ThenContainsExactlyFourRoles()
    {
        Assert.That(AdminRole.Enumerations.Values, Is.EquivalentTo(new[]
        {
            AdminRole.Admin, AdminRole.Staff, AdminRole.Maintainer, AdminRole.SystemAdmin
        }));
    }
}

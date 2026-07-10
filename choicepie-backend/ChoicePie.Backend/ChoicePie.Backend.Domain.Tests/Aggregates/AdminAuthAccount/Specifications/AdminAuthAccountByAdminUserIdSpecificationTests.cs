using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Specifications;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using AdminAuthAccountAggregate = ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.AdminAuthAccount;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.AdminAuthAccount.Specifications;

[TestFixture]
public class AdminAuthAccountByAdminUserIdSpecificationTests
{
    [Test]
    public void ToExpression_GivenAdminAuthAccountWithMatchingAdminUserId_WhenCompiledAndInvoked_ThenReturnsTrue()
    {
        var adminUserId = Guid.NewGuid();
        var adminAuthAccount = AdminAuthAccountAggregate.Create(Email.Create("admin@example.com"), "hashed-password", adminUserId);
        var specification = new AdminAuthAccountByAdminUserIdSpecification(adminUserId);

        var isMatch = specification.ToExpression().Compile()(adminAuthAccount);

        Assert.That(isMatch, Is.True);
    }

    [Test]
    public void ToExpression_GivenAdminAuthAccountWithDifferentAdminUserId_WhenCompiledAndInvoked_ThenReturnsFalse()
    {
        var adminAuthAccount = AdminAuthAccountAggregate.Create(Email.Create("admin@example.com"), "hashed-password", Guid.NewGuid());
        var specification = new AdminAuthAccountByAdminUserIdSpecification(Guid.NewGuid());

        var isMatch = specification.ToExpression().Compile()(adminAuthAccount);

        Assert.That(isMatch, Is.False);
    }
}

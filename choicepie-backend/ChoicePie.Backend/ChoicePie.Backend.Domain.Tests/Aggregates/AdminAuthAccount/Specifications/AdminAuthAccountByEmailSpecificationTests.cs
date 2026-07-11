using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Specifications;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using AdminAuthAccountAggregate = ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.AdminAuthAccount;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.AdminAuthAccount.Specifications;

[TestFixture]
public class AdminAuthAccountByEmailSpecificationTests
{
    [Test]
    public void ToExpression_GivenAdminAuthAccountWithMatchingEmail_WhenCompiledAndInvoked_ThenReturnsTrue()
    {
        var email = Email.Create("admin@example.com");
        var adminAuthAccount = AdminAuthAccountAggregate.Create(email, "hashed-password", "salt", Guid.NewGuid());
        var specification = new AdminAuthAccountByEmailSpecification(email);

        var isMatch = specification.ToExpression().Compile()(adminAuthAccount);

        Assert.That(isMatch, Is.True);
    }

    [Test]
    public void ToExpression_GivenAdminAuthAccountWithDifferentEmail_WhenCompiledAndInvoked_ThenReturnsFalse()
    {
        var adminAuthAccount =
            AdminAuthAccountAggregate.Create(Email.Create("admin@example.com"), "hashed-password", "salt",
                Guid.NewGuid());
        var specification = new AdminAuthAccountByEmailSpecification(Email.Create("someone-else@example.com"));

        var isMatch = specification.ToExpression().Compile()(adminAuthAccount);

        Assert.That(isMatch, Is.False);
    }
}
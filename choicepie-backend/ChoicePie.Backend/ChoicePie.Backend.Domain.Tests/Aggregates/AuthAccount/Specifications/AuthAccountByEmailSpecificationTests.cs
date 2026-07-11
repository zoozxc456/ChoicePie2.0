using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using AuthAccountAggregate = ChoicePie.Backend.Domain.Aggregates.AuthAccount.AuthAccount;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.AuthAccount.Specifications;

[TestFixture]
public class AuthAccountByEmailSpecificationTests
{
    [Test]
    public void ToExpression_GivenAuthAccountWithMatchingEmail_WhenCompiledAndInvoked_ThenReturnsTrue()
    {
        var email = Email.Create("host@example.com");
        var authAccount = AuthAccountAggregate.Register(email, "hashed-password", "salt", Guid.NewGuid());
        var specification = new AuthAccountByEmailSpecification(email);

        var isMatch = specification.ToExpression().Compile()(authAccount);

        Assert.That(isMatch, Is.True);
    }

    [Test]
    public void ToExpression_GivenAuthAccountWithDifferentEmail_WhenCompiledAndInvoked_ThenReturnsFalse()
    {
        var authAccount =
            AuthAccountAggregate.Register(Email.Create("host@example.com"), "hashed-password", "salt", Guid.NewGuid());
        var specification = new AuthAccountByEmailSpecification(Email.Create("someone-else@example.com"));

        var isMatch = specification.ToExpression().Compile()(authAccount);

        Assert.That(isMatch, Is.False);
    }
}
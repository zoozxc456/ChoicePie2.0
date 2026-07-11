using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using AuthAccountAggregate = ChoicePie.Backend.Domain.Aggregates.AuthAccount.AuthAccount;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.AuthAccount.Specifications;

[TestFixture]
public class AuthAccountByMemberIdSpecificationTests
{
    [Test]
    public void ToExpression_GivenAuthAccountWithMatchingMemberId_WhenCompiledAndInvoked_ThenReturnsTrue()
    {
        var memberId = Guid.NewGuid();
        var authAccount =
            AuthAccountAggregate.Register(Email.Create("host@example.com"), HashedPassword.Create("hashed-password", "salt"), memberId);
        var specification = new AuthAccountByMemberIdSpecification(memberId);

        var isMatch = specification.ToExpression().Compile()(authAccount);

        Assert.That(isMatch, Is.True);
    }

    [Test]
    public void ToExpression_GivenAuthAccountWithDifferentMemberId_WhenCompiledAndInvoked_ThenReturnsFalse()
    {
        var authAccount =
            AuthAccountAggregate.Register(Email.Create("host@example.com"), HashedPassword.Create("hashed-password", "salt"), Guid.NewGuid());
        var specification = new AuthAccountByMemberIdSpecification(Guid.NewGuid());

        var isMatch = specification.ToExpression().Compile()(authAccount);

        Assert.That(isMatch, Is.False);
    }
}
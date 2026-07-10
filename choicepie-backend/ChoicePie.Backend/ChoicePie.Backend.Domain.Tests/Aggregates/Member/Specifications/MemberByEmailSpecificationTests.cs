using ChoicePie.Backend.Domain.Aggregates.Member.Specifications;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using MemberAggregate = ChoicePie.Backend.Domain.Aggregates.Member.Member;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.Member.Specifications;

[TestFixture]
public class MemberByEmailSpecificationTests
{
    [Test]
    public void ToExpression_GivenMemberWithMatchingEmail_WhenCompiledAndInvoked_ThenReturnsTrue()
    {
        var email = Email.Create("host@example.com");
        var member = MemberAggregate.Register(email, "Host Name", "hashed-password");
        var specification = new MemberByEmailSpecification(email);

        var isMatch = specification.ToExpression().Compile()(member);

        Assert.That(isMatch, Is.True);
    }

    [Test]
    public void ToExpression_GivenMemberWithDifferentEmail_WhenCompiledAndInvoked_ThenReturnsFalse()
    {
        var member = MemberAggregate.Register(Email.Create("host@example.com"), "Host Name", "hashed-password");
        var specification = new MemberByEmailSpecification(Email.Create("someone-else@example.com"));

        var isMatch = specification.ToExpression().Compile()(member);

        Assert.That(isMatch, Is.False);
    }
}

using ChoicePie.Backend.Domain.Aggregates.MembershipTier.Specifications;
using MembershipTierAggregate = ChoicePie.Backend.Domain.Aggregates.MembershipTier.MembershipTier;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.MembershipTier.Specifications;

[TestFixture]
public class DefaultMembershipTierSpecificationTests
{
    [TestCase(true, ExpectedResult = true)]
    [TestCase(false, ExpectedResult = false)]
    public bool ToExpression_GivenTier_WhenCompiledAndInvoked_ThenMatchesOnlyDefaultTier(bool isDefault)
    {
        var tier = MembershipTierAggregate.Create("Free", 3, 10_000, isDefault);

        return new DefaultMembershipTierSpecification().ToExpression().Compile()(tier);
    }
}

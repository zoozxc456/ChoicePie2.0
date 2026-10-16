using ChoicePie.Backend.Domain.Aggregates.MembershipTier.Exceptions;
using MembershipTierAggregate = ChoicePie.Backend.Domain.Aggregates.MembershipTier.MembershipTier;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.MembershipTier;

[TestFixture]
public class MembershipTierTests
{
    [Test]
    public void Create_GivenValidInput_WhenCalled_ThenCreatesTierWithExpectedFields()
    {
        var tier = MembershipTierAggregate.Create("Free", 3, 10_000, isDefault: true);

        Assert.Multiple(() =>
        {
            Assert.That(tier.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(tier.Name, Is.EqualTo("Free"));
            Assert.That(tier.DailyGenerationLimit, Is.EqualTo(3));
            Assert.That(tier.DailyTokenBudget, Is.EqualTo(10_000));
            Assert.That(tier.IsDefault, Is.True);
        });
    }

    [Test]
    public void Create_GivenNameWithSurroundingWhitespace_WhenCalled_ThenTrimsName()
    {
        var tier = MembershipTierAggregate.Create("  Pro  ", 10, 50_000);

        Assert.That(tier.Name, Is.EqualTo("Pro"));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Create_GivenEmptyOrWhitespaceName_WhenCalled_ThenThrowsInvalidMembershipTierException(string name)
    {
        Assert.Throws<InvalidMembershipTierException>(() => MembershipTierAggregate.Create(name, 3, 10_000));
    }

    [Test]
    public void Create_GivenNameExceedingMaxLength_WhenCalled_ThenThrowsInvalidMembershipTierException()
    {
        var tooLong = new string('a', 51);

        Assert.Throws<InvalidMembershipTierException>(() => MembershipTierAggregate.Create(tooLong, 3, 10_000));
    }

    [TestCase(-1, 10_000)]
    [TestCase(3, -1)]
    public void Create_GivenNegativeLimits_WhenCalled_ThenThrowsInvalidMembershipTierException(
        int dailyGenerationLimit, int dailyTokenBudget)
    {
        Assert.Throws<InvalidMembershipTierException>(() =>
            MembershipTierAggregate.Create("Free", dailyGenerationLimit, dailyTokenBudget));
    }

    [Test]
    public void Update_GivenValidInput_WhenCalled_ThenUpdatesFields()
    {
        var tier = MembershipTierAggregate.Create("Free", 3, 10_000);

        tier.Update("Pro", 10, 50_000);

        Assert.Multiple(() =>
        {
            Assert.That(tier.Name, Is.EqualTo("Pro"));
            Assert.That(tier.DailyGenerationLimit, Is.EqualTo(10));
            Assert.That(tier.DailyTokenBudget, Is.EqualTo(50_000));
        });
    }

    [Test]
    public void Update_GivenInvalidInput_WhenCalled_ThenThrowsAndLeavesFieldsUnchanged()
    {
        var tier = MembershipTierAggregate.Create("Free", 3, 10_000);

        Assert.Throws<InvalidMembershipTierException>(() => tier.Update("", 10, 50_000));

        Assert.Multiple(() =>
        {
            Assert.That(tier.Name, Is.EqualTo("Free"));
            Assert.That(tier.DailyGenerationLimit, Is.EqualTo(3));
            Assert.That(tier.DailyTokenBudget, Is.EqualTo(10_000));
        });
    }
}

using AiUsageLogAggregate = ChoicePie.Backend.Domain.Aggregates.AiUsageLog.AiUsageLog;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.AiUsageLog;

[TestFixture]
public class AiUsageLogTests
{
    [Test]
    public void Create_GivenValidInput_WhenCalled_ThenCreatesLogWithExpectedFields()
    {
        var memberId = Guid.NewGuid();

        var log = AiUsageLogAggregate.Create(memberId, "Anthropic", "claude-sonnet-5", 150);

        Assert.Multiple(() =>
        {
            Assert.That(log.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(log.MemberId, Is.EqualTo(memberId));
            Assert.That(log.Provider, Is.EqualTo("Anthropic"));
            Assert.That(log.Model, Is.EqualTo("claude-sonnet-5"));
            Assert.That(log.TokensUsed, Is.EqualTo(150));
            Assert.That(log.CreatorId, Is.EqualTo(memberId));
        });
    }
}

using ChoicePie.Backend.Domain.Aggregates.AiUsageLog;
using ChoicePie.Backend.Infrastructure.QueryServices.AiUsage;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using NSubstitute;

namespace ChoicePie.Backend.Infrastructure.Tests.QueryServices.AiUsage;

[TestFixture]
public class AiUsageLogQueryServiceTests
{
    private IReadRepository _readRepository = null!;
    private TimeProvider _timeProvider = null!;
    private AiUsageLogQueryService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _readRepository = Substitute.For<IReadRepository>();
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(DateTimeOffset.UtcNow);
        _sut = new AiUsageLogQueryService(_readRepository, _timeProvider);
    }

    [Test]
    public async Task AdminGetByMemberIdAsync_GivenLogsForMultipleMembers_WhenCalled_ThenAggregatesOnlyRequestedMember()
    {
        var memberId = Guid.NewGuid();
        var otherMemberId = Guid.NewGuid();

        var logs = new List<AiUsageLog>
        {
            AiUsageLog.Create(memberId, "Anthropic", "claude-sonnet-5", 100),
            AiUsageLog.Create(memberId, "Anthropic", "claude-sonnet-5", 50),
            AiUsageLog.Create(otherMemberId, "Anthropic", "claude-sonnet-5", 999)
        };
        _readRepository.Query<AiUsageLog>().Returns(logs.AsQueryable());

        var result = await _sut.AdminGetByMemberIdAsync(memberId, recentLogCount: 20, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.MemberId, Is.EqualTo(memberId));
            Assert.That(result.TotalTokensUsed, Is.EqualTo(150));
            Assert.That(result.TotalGenerationCount, Is.EqualTo(2));
            Assert.That(result.RecentLogs, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task AdminGetByMemberIdAsync_GivenNoLogs_WhenCalled_ThenReturnsZeroedSummary()
    {
        _readRepository.Query<AiUsageLog>().Returns(new List<AiUsageLog>().AsQueryable());

        var result = await _sut.AdminGetByMemberIdAsync(Guid.NewGuid(), recentLogCount: 20, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalTokensUsed, Is.EqualTo(0));
            Assert.That(result.TotalGenerationCount, Is.EqualTo(0));
            Assert.That(result.TodayGenerationCount, Is.EqualTo(0));
            Assert.That(result.TodayTokensUsed, Is.EqualTo(0));
            Assert.That(result.RecentLogs, Is.Empty);
        });
    }

    [Test]
    public async Task AdminGetByMemberIdAsync_GivenMoreLogsThanRecentLogCount_WhenCalled_ThenReturnsOnlyMostRecent()
    {
        var memberId = Guid.NewGuid();
        var logs = Enumerable.Range(0, 5)
            .Select(_ => AiUsageLog.Create(memberId, "Anthropic", "claude-sonnet-5", 10))
            .ToList();
        _readRepository.Query<AiUsageLog>().Returns(logs.AsQueryable());

        var result = await _sut.AdminGetByMemberIdAsync(memberId, recentLogCount: 2, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalGenerationCount, Is.EqualTo(5));
            Assert.That(result.RecentLogs, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task AdminGetByMemberIdAsync_GivenLogsFromTodayAndYesterday_WhenCalled_ThenOnlyCountsTodayInTodayTotals()
    {
        var memberId = Guid.NewGuid();
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var todayLog = AiUsageLog.Create(memberId, "Anthropic", "claude-sonnet-5", 100);
        var yesterdayLog = AiUsageLog.Create(memberId, "Anthropic", "claude-sonnet-5", 999);
        typeof(AiUsageLog).GetProperty("CreatedAt")!.SetValue(yesterdayLog, now.AddDays(-1));
        _readRepository.Query<AiUsageLog>().Returns(new List<AiUsageLog> { todayLog, yesterdayLog }.AsQueryable());

        var result = await _sut.AdminGetByMemberIdAsync(memberId, recentLogCount: 20, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalGenerationCount, Is.EqualTo(2));
            Assert.That(result.TotalTokensUsed, Is.EqualTo(1099));
            Assert.That(result.TodayGenerationCount, Is.EqualTo(1));
            Assert.That(result.TodayTokensUsed, Is.EqualTo(100));
        });
    }
}

using ChoicePie.Backend.Domain.Aggregates.QuizReport;
using ChoicePie.Backend.Domain.Aggregates.QuizReport.Enums;
using ChoicePie.Backend.Infrastructure.QueryServices.QuizReports;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using NSubstitute;

namespace ChoicePie.Backend.Infrastructure.Tests.QueryServices.QuizReports;

[TestFixture]
public class QuizReportQueryServiceTests
{
    private IReadRepository _readRepository = null!;
    private QuizReportQueryService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _readRepository = Substitute.For<IReadRepository>();
        _sut = new QuizReportQueryService(_readRepository);
    }

    [Test]
    public async Task AdminGetPendingCountAsync_GivenMixOfStatuses_WhenCalled_ThenCountsOnlyPending()
    {
        var pending = QuizReport.Create(Guid.NewGuid(), Guid.NewGuid(), ReportReason.Spam, null);
        var resolved = QuizReport.Create(Guid.NewGuid(), Guid.NewGuid(), ReportReason.Spam, null);
        resolved.Resolve(Guid.NewGuid(), null, DateTime.UtcNow);
        var dismissed = QuizReport.Create(Guid.NewGuid(), Guid.NewGuid(), ReportReason.Spam, null);
        dismissed.Dismiss(Guid.NewGuid(), null, DateTime.UtcNow);
        _readRepository.Query<QuizReport>().Returns(new List<QuizReport> { pending, resolved, dismissed }.AsQueryable());

        var result = await _sut.AdminGetPendingCountAsync(CancellationToken.None);

        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public async Task AdminGetPendingCountAsync_GivenNoReports_WhenCalled_ThenReturnsZero()
    {
        _readRepository.Query<QuizReport>().Returns(new List<QuizReport>().AsQueryable());

        var result = await _sut.AdminGetPendingCountAsync(CancellationToken.None);

        Assert.That(result, Is.EqualTo(0));
    }
}

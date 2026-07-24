using ChoicePie.Backend.Domain.Aggregates.QuizReport.Enums;
using ChoicePie.Backend.Domain.Aggregates.QuizReport.Exceptions;
using QuizReportAggregate = ChoicePie.Backend.Domain.Aggregates.QuizReport.QuizReport;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.QuizReport;

[TestFixture]
public class QuizReportTests
{
    [Test]
    public void Create_GivenValidInput_WhenCalled_ThenCreatesReportWithExpectedFields()
    {
        var quizId = Guid.NewGuid();
        var reporterId = Guid.NewGuid();

        var report = QuizReportAggregate.Create(quizId, reporterId, ReportReason.Spam, "spam content");

        Assert.Multiple(() =>
        {
            Assert.That(report.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(report.QuizId, Is.EqualTo(quizId));
            Assert.That(report.ReporterId, Is.EqualTo(reporterId));
            Assert.That(report.Reason, Is.EqualTo(ReportReason.Spam));
            Assert.That(report.Description, Is.EqualTo("spam content"));
            Assert.That(report.Status, Is.EqualTo(ReportStatus.Pending));
        });
    }

    [Test]
    public void Create_GivenNullDescription_WhenCalled_ThenDescriptionIsNull()
    {
        var report = QuizReportAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), ReportReason.Other, null);

        Assert.That(report.Description, Is.Null);
    }

    [Test]
    public void Create_GivenDescriptionExceedingMaxLength_WhenCalled_ThenThrowsInvalidQuizReportException()
    {
        var tooLong = new string('a', 501);

        Assert.Throws<InvalidQuizReportException>(() =>
            QuizReportAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), ReportReason.Other, tooLong));
    }

    [Test]
    public void Resolve_GivenPendingReport_WhenCalled_ThenSetsResolvedStateAndFields()
    {
        var report = QuizReportAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), ReportReason.Spam, null);
        var adminId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        report.Resolve(adminId, "removed", now);

        Assert.Multiple(() =>
        {
            Assert.That(report.Status, Is.EqualTo(ReportStatus.Resolved));
            Assert.That(report.ResolvedBy, Is.EqualTo(adminId));
            Assert.That(report.ResolvedAt, Is.EqualTo(now));
            Assert.That(report.ResolutionNote, Is.EqualTo("removed"));
        });
    }

    [Test]
    public void Dismiss_GivenPendingReport_WhenCalled_ThenSetsDismissedStateAndFields()
    {
        var report = QuizReportAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), ReportReason.Spam, null);
        var adminId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        report.Dismiss(adminId, "not a violation", now);

        Assert.Multiple(() =>
        {
            Assert.That(report.Status, Is.EqualTo(ReportStatus.Dismissed));
            Assert.That(report.ResolvedBy, Is.EqualTo(adminId));
            Assert.That(report.ResolvedAt, Is.EqualTo(now));
            Assert.That(report.ResolutionNote, Is.EqualTo("not a violation"));
        });
    }

    [Test]
    public void Resolve_GivenAlreadyResolvedReport_WhenCalled_ThenThrowsInvalidQuizReportException()
    {
        var report = QuizReportAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), ReportReason.Spam, null);
        report.Resolve(Guid.NewGuid(), null, DateTime.UtcNow);

        Assert.Throws<InvalidQuizReportException>(() => report.Resolve(Guid.NewGuid(), null, DateTime.UtcNow));
    }

    [Test]
    public void Dismiss_GivenAlreadyDismissedReport_WhenCalled_ThenThrowsInvalidQuizReportException()
    {
        var report = QuizReportAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), ReportReason.Spam, null);
        report.Dismiss(Guid.NewGuid(), null, DateTime.UtcNow);

        Assert.Throws<InvalidQuizReportException>(() => report.Dismiss(Guid.NewGuid(), null, DateTime.UtcNow));
    }

    [Test]
    public void Resolve_GivenResolutionNoteExceedingMaxLength_WhenCalled_ThenThrowsInvalidQuizReportException()
    {
        var report = QuizReportAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), ReportReason.Spam, null);
        var tooLong = new string('a', 501);

        Assert.Throws<InvalidQuizReportException>(() => report.Resolve(Guid.NewGuid(), tooLong, DateTime.UtcNow));
    }
}

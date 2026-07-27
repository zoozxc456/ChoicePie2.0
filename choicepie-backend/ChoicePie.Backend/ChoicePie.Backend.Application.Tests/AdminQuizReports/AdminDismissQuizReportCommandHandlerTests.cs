using ChoicePie.Backend.Application.AdminQuizReports.Commands;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.QuizReport;
using ChoicePie.Backend.Domain.Aggregates.QuizReport.Enums;
using ChoicePie.Backend.Domain.Aggregates.QuizReport.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;
using QuizReportAggregate = ChoicePie.Backend.Domain.Aggregates.QuizReport.QuizReport;

namespace ChoicePie.Backend.Application.Tests.AdminQuizReports;

[TestFixture]
public class AdminDismissQuizReportCommandHandlerTests
{
    private IQuizReportRepository _quizReportRepository = null!;
    private ICurrentAdminUserService _currentAdminUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private TimeProvider _timeProvider = null!;
    private AdminDismissQuizReportCommandHandler _sut = null!;
    private readonly Guid _adminId = Guid.NewGuid();
    private QuizReportAggregate _report = null!;

    [SetUp]
    public void SetUp()
    {
        _quizReportRepository = Substitute.For<IQuizReportRepository>();
        _currentAdminUserService = Substitute.For<ICurrentAdminUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(DateTimeOffset.UtcNow);
        _sut = new AdminDismissQuizReportCommandHandler(_quizReportRepository, _currentAdminUserService, _unitOfWork, _timeProvider);

        _report = QuizReportAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), ReportReason.Spam, null);
        _quizReportRepository.GetByIdAsync(_report.Id, Arg.Any<CancellationToken>()).Returns(_report);
        _currentAdminUserService.AdminUserId.Returns(_adminId);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenPendingReport_WhenCalled_ThenDismissesAndPersists()
    {
        await _sut.Handle(new AdminDismissQuizReportCommand(_report.Id, "not a violation"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(_report.Status, Is.EqualTo(ReportStatus.Dismissed));
            Assert.That(_report.ResolvedBy, Is.EqualTo(_adminId));
        });
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenReportNotFound_WhenCalled_ThenThrowsQuizReportNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _quizReportRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((QuizReportAggregate?)null);

        Assert.ThrowsAsync<QuizReportNotFoundException>(() =>
            _sut.Handle(new AdminDismissQuizReportCommand(missingId, null), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenNoCurrentAdminUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentAdminUserService.AdminUserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() =>
            _sut.Handle(new AdminDismissQuizReportCommand(_report.Id, null), CancellationToken.None));
    }
}

using ChoicePie.Backend.Application.AdminQuizReports.Commands;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.QuizReport;
using ChoicePie.Backend.Domain.Aggregates.QuizReport.Enums;
using ChoicePie.Backend.Domain.Aggregates.QuizReport.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;
using QuizReportAggregate = ChoicePie.Backend.Domain.Aggregates.QuizReport.QuizReport;

namespace ChoicePie.Backend.Application.Tests.AdminQuizReports;

[TestFixture]
public class AdminResolveQuizReportCommandHandlerTests
{
    private IQuizReportRepository _quizReportRepository = null!;
    private IQuizRepository _quizRepository = null!;
    private ICurrentAdminUserService _currentAdminUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private TimeProvider _timeProvider = null!;
    private AdminResolveQuizReportCommandHandler _sut = null!;
    private readonly Guid _adminId = Guid.NewGuid();
    private Quiz _quiz = null!;
    private QuizReportAggregate _report = null!;

    [SetUp]
    public void SetUp()
    {
        _quizReportRepository = Substitute.For<IQuizReportRepository>();
        _quizRepository = Substitute.For<IQuizRepository>();
        _currentAdminUserService = Substitute.For<ICurrentAdminUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(DateTimeOffset.UtcNow);
        _sut = new AdminResolveQuizReportCommandHandler(
            _quizReportRepository, _quizRepository, _currentAdminUserService, _unitOfWork, _timeProvider);

        _quiz = Quiz.Create(Guid.NewGuid(), "Title", null, "⚓", "g", Difficulty.Beginner, []);
        _quiz.AddQuestion(Question.Create("2+2=?", ["1", "2", "3", "4"], 3, "basic math"));
        _quiz.Publish();
        _report = QuizReportAggregate.Create(_quiz.Id, Guid.NewGuid(), ReportReason.Spam, null);

        _quizReportRepository.GetByIdAsync(_report.Id, Arg.Any<CancellationToken>()).Returns(_report);
        _quizRepository.GetByIdAsync(_quiz.Id, Arg.Any<CancellationToken>()).Returns(_quiz);
        _currentAdminUserService.AdminUserId.Returns(_adminId);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenPendingReport_WhenCalled_ThenResolvesReportAndTakesDownQuiz()
    {
        await _sut.Handle(new AdminResolveQuizReportCommand(_report.Id, "removed"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(_report.Status, Is.EqualTo(ReportStatus.Resolved));
            Assert.That(_report.ResolvedBy, Is.EqualTo(_adminId));
            Assert.That(_quiz.Status, Is.EqualTo(QuizStatus.TakenDown));
        });
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenReportNotFound_WhenCalled_ThenThrowsQuizReportNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _quizReportRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((QuizReportAggregate?)null);

        Assert.ThrowsAsync<QuizReportNotFoundException>(() =>
            _sut.Handle(new AdminResolveQuizReportCommand(missingId, null), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenQuizNotFound_WhenCalled_ThenThrowsQuizNotFoundException()
    {
        _quizRepository.GetByIdAsync(_quiz.Id, Arg.Any<CancellationToken>()).Returns((Quiz?)null);

        Assert.ThrowsAsync<QuizNotFoundException>(() =>
            _sut.Handle(new AdminResolveQuizReportCommand(_report.Id, null), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenNoCurrentAdminUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentAdminUserService.AdminUserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() =>
            _sut.Handle(new AdminResolveQuizReportCommand(_report.Id, null), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenAlreadyResolvedReport_WhenCalled_ThenThrowsInvalidQuizReportException()
    {
        _report.Resolve(Guid.NewGuid(), null, DateTime.UtcNow);

        Assert.ThrowsAsync<InvalidQuizReportException>(() =>
            _sut.Handle(new AdminResolveQuizReportCommand(_report.Id, null), CancellationToken.None));
    }
}

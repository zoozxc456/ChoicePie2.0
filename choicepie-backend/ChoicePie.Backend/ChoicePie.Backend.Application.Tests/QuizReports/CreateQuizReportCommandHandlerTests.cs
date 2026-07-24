using ChoicePie.Backend.Application.QuizReports.Commands;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.QuizReport;
using ChoicePie.Backend.Domain.Aggregates.QuizReport.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.QuizReports;

[TestFixture]
public class CreateQuizReportCommandHandlerTests
{
    private IQuizReportRepository _quizReportRepository = null!;
    private IQuizRepository _quizRepository = null!;
    private ICurrentUserService _currentUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private CreateQuizReportCommandHandler _sut = null!;
    private readonly Guid _userId = Guid.NewGuid();
    private Quiz _quiz = null!;

    [SetUp]
    public void SetUp()
    {
        _quizReportRepository = Substitute.For<IQuizReportRepository>();
        _quizRepository = Substitute.For<IQuizRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new CreateQuizReportCommandHandler(_quizReportRepository, _quizRepository, _currentUserService, _unitOfWork);

        _quiz = Quiz.Create(Guid.NewGuid(), "Title", null, "⚓", "g", Difficulty.Beginner, []);
        _quizRepository.GetByIdAsync(_quiz.Id, Arg.Any<CancellationToken>()).Returns(_quiz);
        _currentUserService.UserId.Returns(_userId);
        _quizReportRepository.ExistsAsync(Arg.Any<ISpecification<QuizReport>>(), Arg.Any<CancellationToken>()).Returns(false);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenValidRequest_WhenCalled_ThenAddsReportAndPersists()
    {
        await _sut.Handle(new CreateQuizReportCommand(_quiz.Id, "Spam", "spam desc"), CancellationToken.None);

        await _quizReportRepository.Received(1).AddAsync(Arg.Any<QuizReport>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenQuizNotFound_WhenCalled_ThenThrowsQuizNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _quizRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((Quiz?)null);

        Assert.ThrowsAsync<QuizNotFoundException>(() =>
            _sut.Handle(new CreateQuizReportCommand(missingId, "Spam", null), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenUnauthenticatedUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() =>
            _sut.Handle(new CreateQuizReportCommand(_quiz.Id, "Spam", null), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenInvalidReason_WhenCalled_ThenThrowsInvalidQuizReportException()
    {
        Assert.ThrowsAsync<InvalidQuizReportException>(() =>
            _sut.Handle(new CreateQuizReportCommand(_quiz.Id, "NotAReason", null), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenPendingReportAlreadyExists_WhenCalled_ThenThrowsInvalidQuizReportException()
    {
        _quizReportRepository.ExistsAsync(Arg.Any<ISpecification<QuizReport>>(), Arg.Any<CancellationToken>()).Returns(true);

        Assert.ThrowsAsync<InvalidQuizReportException>(() =>
            _sut.Handle(new CreateQuizReportCommand(_quiz.Id, "Spam", null), CancellationToken.None));
    }
}

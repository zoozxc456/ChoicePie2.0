using ChoicePie.Backend.Application.AdminQuizzes.Commands;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.AdminQuizzes;

[TestFixture]
public class AdminTakeDownQuizCommandHandlerTests
{
    private IQuizRepository _quizRepository = null!;
    private ICurrentAdminUserService _currentAdminUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private TimeProvider _timeProvider = null!;
    private AdminTakeDownQuizCommandHandler _sut = null!;
    private readonly Guid _adminId = Guid.NewGuid();
    private Quiz _quiz = null!;

    [SetUp]
    public void SetUp()
    {
        _quizRepository = Substitute.For<IQuizRepository>();
        _currentAdminUserService = Substitute.For<ICurrentAdminUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(DateTimeOffset.UtcNow);
        _sut = new AdminTakeDownQuizCommandHandler(_quizRepository, _currentAdminUserService, _unitOfWork, _timeProvider);

        _quiz = Quiz.Create(Guid.NewGuid(), "Title", null, "⚓", "g", Difficulty.Beginner, []);
        _quiz.AddQuestion(Question.Create("2+2=?", ["1", "2", "3", "4"], 3, "basic math"));
        _quiz.Publish();
        _quizRepository.GetByIdAsync(_quiz.Id, Arg.Any<CancellationToken>()).Returns(_quiz);
        _currentAdminUserService.AdminUserId.Returns(_adminId);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenValidReason_WhenCalled_ThenTakesDownAndPersists()
    {
        await _sut.Handle(new AdminTakeDownQuizCommand(_quiz.Id, "inappropriate content"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(_quiz.Status, Is.EqualTo(QuizStatus.TakenDown));
            Assert.That(_quiz.TakedownReason, Is.EqualTo("inappropriate content"));
            Assert.That(_quiz.TakedownBy, Is.EqualTo(_adminId));
        });
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenQuizNotFound_WhenCalled_ThenThrowsQuizNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _quizRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((Quiz?)null);

        Assert.ThrowsAsync<QuizNotFoundException>(() =>
            _sut.Handle(new AdminTakeDownQuizCommand(missingId, "reason"), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenNoCurrentAdminUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentAdminUserService.AdminUserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() =>
            _sut.Handle(new AdminTakeDownQuizCommand(_quiz.Id, "reason"), CancellationToken.None));
    }
}

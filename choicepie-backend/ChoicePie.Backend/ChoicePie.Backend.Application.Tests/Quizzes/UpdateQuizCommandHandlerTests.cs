using ChoicePie.Backend.Application.Quizzes.Commands;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class UpdateQuizCommandHandlerTests
{
    private IRepository<Quiz> _quizRepository = null!;
    private IMemberRepository _memberRepository = null!;
    private ICurrentUserService _currentUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private UpdateQuizCommandHandler _sut = null!;
    private readonly Guid _ownerId = Guid.NewGuid();
    private Quiz _quiz = null!;

    [SetUp]
    public void SetUp()
    {
        _quizRepository = Substitute.For<IRepository<Quiz>>();
        _memberRepository = Substitute.For<IMemberRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new UpdateQuizCommandHandler(_quizRepository, _memberRepository, _currentUserService, _unitOfWork);

        _quiz = Quiz.Create(_ownerId, "Old Title", null, "⚓", "g", Difficulty.Beginner, true, ["Old"]);
        _quizRepository.GetByIdAsync(_quiz.Id, Arg.Any<CancellationToken>()).Returns(_quiz);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    private UpdateQuizCommand ValidCommand() => new()
    {
        Id = _quiz.Id,
        Title = "New Title",
        Description = "New Description",
        IsPublic = false,
        Tags = ["New"]
    };

    [Test]
    public async Task Handle_GivenOwner_WhenCalled_ThenUpdatesQuizAndPersists()
    {
        _currentUserService.UserId.Returns(_ownerId);

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.EqualTo("New Title"));
            Assert.That(result.IsPublic, Is.False);
        });
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenNonOwner_WhenCalled_ThenThrowsQuizForbiddenException()
    {
        _currentUserService.UserId.Returns(Guid.NewGuid());

        Assert.ThrowsAsync<QuizForbiddenException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenQuizNotFound_WhenCalled_ThenThrowsQuizNotFoundException()
    {
        _currentUserService.UserId.Returns(_ownerId);
        var command = new UpdateQuizCommand
        {
            Id = Guid.NewGuid(),
            Title = "New Title",
            Description = null,
            IsPublic = true,
            Tags = []
        };
        _quizRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((Quiz?)null);

        Assert.ThrowsAsync<QuizNotFoundException>(() => _sut.Handle(command, CancellationToken.None));
    }
}

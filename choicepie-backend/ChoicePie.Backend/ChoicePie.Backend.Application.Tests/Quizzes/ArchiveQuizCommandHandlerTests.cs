using ChoicePie.Backend.Application.Quizzes.Commands;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class ArchiveQuizCommandHandlerTests
{
    private IQuizRepository _quizRepository = null!;
    private IMemberRepository _memberRepository = null!;
    private ICurrentUserService _currentUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private ArchiveQuizCommandHandler _sut = null!;
    private readonly Guid _ownerId = Guid.NewGuid();
    private Quiz _quiz = null!;

    [SetUp]
    public void SetUp()
    {
        _quizRepository = Substitute.For<IQuizRepository>();
        _memberRepository = Substitute.For<IMemberRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new ArchiveQuizCommandHandler(_quizRepository, _memberRepository, _currentUserService, _unitOfWork);

        _quiz = Quiz.Create(_ownerId, "Title", null, "⚓", "g", Difficulty.Beginner, []);
        _quizRepository.GetByIdAsync(_quiz.Id, Arg.Any<CancellationToken>()).Returns(_quiz);
        _currentUserService.UserId.Returns(_ownerId);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenOwner_WhenCalled_ThenArchivesAndPersists()
    {
        var result = await _sut.Handle(new ArchiveQuizCommand(_quiz.Id), CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo("archived"));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenNonOwner_WhenCalled_ThenThrowsQuizForbiddenException()
    {
        _currentUserService.UserId.Returns(Guid.NewGuid());

        Assert.ThrowsAsync<QuizForbiddenException>(() => _sut.Handle(new ArchiveQuizCommand(_quiz.Id), CancellationToken.None));
    }
}

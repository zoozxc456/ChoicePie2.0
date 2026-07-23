using ChoicePie.Backend.Application.Comments.Commands;
using ChoicePie.Backend.Domain.Aggregates.Comment;
using ChoicePie.Backend.Domain.Aggregates.Comment.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;
using CommentAggregate = ChoicePie.Backend.Domain.Aggregates.Comment.Comment;

namespace ChoicePie.Backend.Application.Tests.Comments;

[TestFixture]
public class AddCommentCommandHandlerTests
{
    private ICommentRepository _commentRepository = null!;
    private IQuizRepository _quizRepository = null!;
    private IMemberRepository _memberRepository = null!;
    private ICurrentUserService _currentUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private AddCommentCommandHandler _sut = null!;
    private readonly Guid _userId = Guid.NewGuid();
    private Quiz _quiz = null!;
    private Member _author = null!;

    [SetUp]
    public void SetUp()
    {
        _commentRepository = Substitute.For<ICommentRepository>();
        _quizRepository = Substitute.For<IQuizRepository>();
        _memberRepository = Substitute.For<IMemberRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new AddCommentCommandHandler(_commentRepository, _quizRepository, _memberRepository, _currentUserService, _unitOfWork);

        _quiz = Quiz.Create(Guid.NewGuid(), "Title", null, "⚓", "g", Difficulty.Beginner, []);
        _author = Member.Create("Alice");
        _quizRepository.GetByIdAsync(_quiz.Id, Arg.Any<CancellationToken>()).Returns(_quiz);
        _memberRepository.GetByIdAsync(_userId, Arg.Any<CancellationToken>()).Returns(_author);
        _currentUserService.UserId.Returns(_userId);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenValidInput_WhenCalled_ThenPersistsAndReturnsDtoWithAuthorName()
    {
        var result = await _sut.Handle(new AddCommentCommand(_quiz.Id, "Nice quiz!"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.QuizId, Is.EqualTo(_quiz.Id));
            Assert.That(result.UserId, Is.EqualTo(_userId));
            Assert.That(result.UserName, Is.EqualTo("Alice"));
            Assert.That(result.Text, Is.EqualTo("Nice quiz!"));
        });
        await _commentRepository.Received(1).AddAsync(Arg.Any<CommentAggregate>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenQuizNotFound_WhenCalled_ThenThrowsQuizNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _quizRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((Quiz?)null);

        Assert.ThrowsAsync<QuizNotFoundException>(() => _sut.Handle(new AddCommentCommand(missingId, "text"), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenEmptyText_WhenCalled_ThenThrowsInvalidCommentTextException()
    {
        Assert.ThrowsAsync<InvalidCommentTextException>(() => _sut.Handle(new AddCommentCommand(_quiz.Id, "  "), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenUnauthenticatedUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() => _sut.Handle(new AddCommentCommand(_quiz.Id, "text"), CancellationToken.None));
    }
}

using ChoicePie.Backend.Application.Comments.Commands;
using ChoicePie.Backend.Domain.Aggregates.Comment;
using ChoicePie.Backend.Domain.Aggregates.Comment.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;
using CommentAggregate = ChoicePie.Backend.Domain.Aggregates.Comment.Comment;

namespace ChoicePie.Backend.Application.Tests.Comments;

[TestFixture]
public class UpdateCommentCommandHandlerTests
{
    private ICommentRepository _commentRepository = null!;
    private IMemberRepository _memberRepository = null!;
    private ICurrentUserService _currentUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private UpdateCommentCommandHandler _sut = null!;
    private readonly Guid _userId = Guid.NewGuid();
    private CommentAggregate _comment = null!;
    private Member _author = null!;

    [SetUp]
    public void SetUp()
    {
        _commentRepository = Substitute.For<ICommentRepository>();
        _memberRepository = Substitute.For<IMemberRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new UpdateCommentCommandHandler(_commentRepository, _memberRepository, _currentUserService, _unitOfWork);

        _comment = CommentAggregate.Create(Guid.NewGuid(), _userId, "original");
        _author = Member.Create("Alice");
        _commentRepository.GetByIdAsync(_comment.Id, Arg.Any<CancellationToken>()).Returns(_comment);
        _memberRepository.GetByIdAsync(_userId, Arg.Any<CancellationToken>()).Returns(_author);
        _currentUserService.UserId.Returns(_userId);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenAuthorAndValidText_WhenCalled_ThenUpdatesAndReturnsDto()
    {
        var result = await _sut.Handle(new UpdateCommentCommand(_comment.Id, "updated text"), CancellationToken.None);

        Assert.That(result.Text, Is.EqualTo("updated text"));
        await _commentRepository.Received(1).UpdateAsync(Arg.Any<CommentAggregate>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenNonAuthor_WhenCalled_ThenThrowsCommentForbiddenException()
    {
        _currentUserService.UserId.Returns(Guid.NewGuid());

        Assert.ThrowsAsync<CommentForbiddenException>(
            () => _sut.Handle(new UpdateCommentCommand(_comment.Id, "updated text"), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenCommentNotFound_WhenCalled_ThenThrowsCommentNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _commentRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((CommentAggregate?)null);

        Assert.ThrowsAsync<CommentNotFoundException>(
            () => _sut.Handle(new UpdateCommentCommand(missingId, "text"), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenUnauthenticatedUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(
            () => _sut.Handle(new UpdateCommentCommand(_comment.Id, "text"), CancellationToken.None));
    }
}

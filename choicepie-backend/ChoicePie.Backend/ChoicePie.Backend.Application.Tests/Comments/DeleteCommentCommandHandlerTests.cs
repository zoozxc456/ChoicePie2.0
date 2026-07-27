using ChoicePie.Backend.Application.Comments.Commands;
using ChoicePie.Backend.Domain.Aggregates.Comment;
using ChoicePie.Backend.Domain.Aggregates.Comment.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;
using CommentAggregate = ChoicePie.Backend.Domain.Aggregates.Comment.Comment;

namespace ChoicePie.Backend.Application.Tests.Comments;

[TestFixture]
public class DeleteCommentCommandHandlerTests
{
    private ICommentRepository _commentRepository = null!;
    private ICurrentUserService _currentUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private DeleteCommentCommandHandler _sut = null!;
    private readonly Guid _userId = Guid.NewGuid();
    private CommentAggregate _comment = null!;

    [SetUp]
    public void SetUp()
    {
        _commentRepository = Substitute.For<ICommentRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new DeleteCommentCommandHandler(_commentRepository, _currentUserService, _unitOfWork);

        _comment = CommentAggregate.Create(Guid.NewGuid(), _userId, "original");
        _commentRepository.GetByIdAsync(_comment.Id, Arg.Any<CancellationToken>()).Returns(_comment);
        _currentUserService.UserId.Returns(_userId);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenAuthor_WhenCalled_ThenSoftDeletesComment()
    {
        await _sut.Handle(new DeleteCommentCommand(_comment.Id), CancellationToken.None);

        Assert.That(_comment.IsDeleted, Is.True);
        await _commentRepository.Received(1).UpdateAsync(_comment, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenNonAuthor_WhenCalled_ThenThrowsCommentForbiddenException()
    {
        _currentUserService.UserId.Returns(Guid.NewGuid());

        Assert.ThrowsAsync<CommentForbiddenException>(
            () => _sut.Handle(new DeleteCommentCommand(_comment.Id), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenCommentNotFound_WhenCalled_ThenThrowsCommentNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _commentRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((CommentAggregate?)null);

        Assert.ThrowsAsync<CommentNotFoundException>(
            () => _sut.Handle(new DeleteCommentCommand(missingId), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenUnauthenticatedUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(
            () => _sut.Handle(new DeleteCommentCommand(_comment.Id), CancellationToken.None));
    }
}

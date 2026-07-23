using ChoicePie.Backend.Domain.Aggregates.Comment;
using ChoicePie.Backend.Domain.Aggregates.Comment.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.Comments.Commands;

public sealed class DeleteCommentCommandHandler(
    ICommentRepository commentRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCommentCommand>
{
    public async Task Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthenticatedException();

        var comment = await commentRepository.GetByIdAsync(request.Id, cancellationToken)
                      ?? throw new CommentNotFoundException(request.Id);

        comment.EnsureModifiableBy(userId);
        comment.Delete(userId);

        await commentRepository.UpdateAsync(comment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
